# 📦 QRay — Custom QR Code Generator in C#

QRay is a fully custom QR code generator written from scratch in C#, implementing:
- ✅ Version-specific layouts (Versions 1–40)
- ✅ Byte mode encoding
- ✅ Full Reed-Solomon error correction (L, M, Q, H)
- ✅ QR block group splitting and interleaving
- ✅ Powerful image rendering using ImageSharp library

> 💡 This project and follows the [ISO/IEC 18004:2015](https://www.iso.org/standard/62021.html) QR Code specification.

---

## 🧠 Why This Project?

Most QR code libraries abstract away the internals. QRay was built to understand, visualize, and control every step of QR generation — from encoding data to rendering raw bitstreams.

---

## ⚙️ Features

| Feature                     | Status         |
|----------------------------|----------------|
| Byte Mode Support          | ✅ Implemented |
| Numeric & Alphanumeric     | 🚧 Not yet     |
| Reed-Solomon ECC           | ✅ Full support|
| Version Control            | ✅ V1–V40      |
| Mask Pattern Evaluation    | ✅ Implemented |
| Image Output               | ✅ ImageSharp  |
| CLI & GUI Support          | 🧪 Planned     |

---

## 🚀 Usage

```csharp
// Example usage
var generator = new QRCode("https://yourlink.com", ErrorCorrectionLevel.M);
var image = generator.RenderImage();
image.Save("qr_output.png");
```

You can also directly access internal structures like the version, blocks, or matrix for debugging/visualization.

---

## 🔒 Limitations

- Only Byte mode is supported right now.
- No structured append or Kanji encoding.
- Rendering is bitmap/grid-based only (no vector/svg export yet).

> Want to contribute? Feel free to fork and submit PRs!

---

## 📁 Project Structure

```
QRay/
├── Components/
│   ├── QRCodeGenerator.cs
│   ├── ECBlockInfo.cs
│   ├── MatrixBuilder.cs
│   └── ...
├── Resources/
│   ├── ECBlockInfos.csv
│   └── QRCapacities.csv
└── Program.cs
```

---

## 🧑‍💻 Author

Built with 💖 by mxte_b.  
Part of my portfolio to showcase low-level C# and systems design knowledge.

---

## 🧪 Planned Features

- [ ] Alphanumeric & Numeric mode
- [ ] Vector output (SVG)
- [ ] Command-line interface
- [ ] WPF/WinForms demo UI
- [ ] Unit test coverage
- [ ] Masking penalty optimization
