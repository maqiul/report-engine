using System;
using System.Collections.Generic;
using ZXing;
using ZXing.Common;

namespace ReportEngine.Core.Barcodes
{
    /// <summary>
    /// 条码生成器：将条码内容 + 格式 → 布尔矩阵 (true=黑点)。
    /// 基于 ZXing.Net (MIT, netstandard1.0)，三 TFM 通用。
    /// </summary>
    public static class BarcodeGenerator
    {
        /// <summary>
        /// 生成条码位图矩阵。
        /// </summary>
        /// <param name="content">条码内容</param>
        /// <param name="format">条码格式</param>
        /// <param name="width">像素宽</param>
        /// <param name="height">像素高</param>
        /// <returns>true=黑色模块</returns>
        public static bool[,] Generate(string content, BarcodeFormat format, int width, int height)
        {
            if (string.IsNullOrEmpty(content))
                return new bool[height, width];

            var zxFormat = MapFormat(format);
            var writer = new BarcodeWriterGeneric
            {
                Format = zxFormat,
                Options = new EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = 1,
                    PureBarcode = false,
                }
            };

            var matrix = writer.Encode(content);
            var result = new bool[matrix.Height, matrix.Width];
            for (int y = 0; y < matrix.Height; y++)
                for (int x = 0; x < matrix.Width; x++)
                    result[y, x] = matrix[x, y];
            return result;
        }

        private static ZXing.BarcodeFormat MapFormat(BarcodeFormat format)
        {
            switch (format)
            {
                case BarcodeFormat.Code128:    return ZXing.BarcodeFormat.CODE_128;
                case BarcodeFormat.Code39:     return ZXing.BarcodeFormat.CODE_39;
                case BarcodeFormat.EAN13:      return ZXing.BarcodeFormat.EAN_13;
                case BarcodeFormat.EAN8:       return ZXing.BarcodeFormat.EAN_8;
                case BarcodeFormat.UPC_A:      return ZXing.BarcodeFormat.UPC_A;
                case BarcodeFormat.QRCode:     return ZXing.BarcodeFormat.QR_CODE;
                case BarcodeFormat.DataMatrix: return ZXing.BarcodeFormat.DATA_MATRIX;
                case BarcodeFormat.PDF417:     return ZXing.BarcodeFormat.PDF_417;
                default:                      return ZXing.BarcodeFormat.QR_CODE;
            }
        }
    }
}
