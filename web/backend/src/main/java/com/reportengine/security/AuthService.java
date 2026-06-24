package com.reportengine.security;

import com.reportengine.user.User;
import com.reportengine.user.UserRepository;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;

import java.util.Map;

@Service
public class AuthService {

    public record AuthResult(boolean success, String token, String username, String role, String error) {}
    public record RegisterRequest(String username, String password, String role) {}
    public record LoginRequest(String username, String password) {}

    private final UserRepository userRepository;
    private final PasswordEncoder passwordEncoder;
    private final JwtService jwtService;

    public AuthService(UserRepository userRepository, PasswordEncoder passwordEncoder, JwtService jwtService) {
        this.userRepository = userRepository;
        this.passwordEncoder = passwordEncoder;
        this.jwtService = jwtService;
    }

    public AuthResult register(RegisterRequest req) {
        if (req == null || req.username() == null || req.password() == null) {
            return new AuthResult(false, null, null, null, "用户名和密码必填");
        }
        String username = req.username().trim();
        if (username.isEmpty() || req.password().length() < 6) {
            return new AuthResult(false, null, null, null, "用户名非空且密码至少 6 位");
        }
        if (userRepository.existsByUsername(username)) {
            return new AuthResult(false, null, null, null, "用户名已存在: " + username);
        }
        User.Role role;
        try {
            role = req.role() == null ? User.Role.USER : User.Role.valueOf(req.role().toUpperCase());
        } catch (IllegalArgumentException e) {
            return new AuthResult(false, null, null, null, "非法角色: " + req.role());
        }
        User u = new User(username, passwordEncoder.encode(req.password()), role);
        userRepository.save(u);
        String token = jwtService.issueToken(username, role.name());
        return new AuthResult(true, token, username, role.name(), null);
    }

    public AuthResult login(LoginRequest req) {
        if (req == null || req.username() == null || req.password() == null) {
            return new AuthResult(false, null, null, null, "用户名和密码必填");
        }
        return userRepository.findByUsername(req.username().trim())
            .filter(u -> passwordEncoder.matches(req.password(), u.getPasswordHash()))
            .map(u -> {
                String token = jwtService.issueToken(u.getUsername(), u.getRole().name());
                return new AuthResult(true, token, u.getUsername(), u.getRole().name(), null);
            })
            .orElse(new AuthResult(false, null, null, null, "用户名或密码错误"));
    }

    public Map<String, Object> me(String username) {
        return userRepository.findByUsername(username)
            .map(u -> Map.<String, Object>of(
                "username", u.getUsername(),
                "role", u.getRole().name(),
                "id", u.getId(),
                "createdAt", u.getCreatedAt().toString()
            ))
            .orElse(Map.of("error", "用户不存在"));
    }
}
