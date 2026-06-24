package com.reportengine.security;

import org.springframework.http.ResponseEntity;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.*;

import java.util.Map;

@RestController
@RequestMapping("/api/auth")
public class AuthController {

    private final AuthService authService;

    public AuthController(AuthService authService) {
        this.authService = authService;
    }

    @PostMapping("/register")
    public ResponseEntity<?> register(@RequestBody AuthService.RegisterRequest req) {
        AuthService.AuthResult r = authService.register(req);
        if (!r.success()) {
            return ResponseEntity.badRequest().body(Map.of("error", r.error()));
        }
        return ResponseEntity.ok(Map.of(
            "token", r.token(),
            "username", r.username(),
            "role", r.role(),
            "tokenType", "Bearer",
            "expiresInMs", 86400000L
        ));
    }

    @PostMapping("/login")
    public ResponseEntity<?> login(@RequestBody AuthService.LoginRequest req) {
        AuthService.AuthResult r = authService.login(req);
        if (!r.success()) {
            return ResponseEntity.status(401).body(Map.of("error", r.error()));
        }
        return ResponseEntity.ok(Map.of(
            "token", r.token(),
            "username", r.username(),
            "role", r.role(),
            "tokenType", "Bearer",
            "expiresInMs", 86400000L
        ));
    }

    @GetMapping("/me")
    public ResponseEntity<?> me(Authentication auth) {
        if (auth == null) {
            return ResponseEntity.status(401).body(Map.of("error", "未登录"));
        }
        return ResponseEntity.ok(authService.me(auth.getName()));
    }
}
