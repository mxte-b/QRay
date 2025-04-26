# 📦 QRay — Custom QR Code Generator in C#

QRay is a fully custom QR code generator written from scratch in C#, implementing:
- ✅ Version-specific layouts (Versions 1–40)
- ✅ Byte mode encoding
- ✅ Full Reed-Solomon error correction (L, M, Q, H)
- ✅ QR block group splitting and interleaving
- ✅ Powerful image rendering using [ImageSharp](https://github.com/SixLabors/ImageSharp)

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
// Retrieving data to be encoded and desired error correction level
string payload = ConsoleInput.GetInput<string>("<#> Text to encode:");
char errorCorrection = ConsoleInput.GetInput<char>("<#> Error Correction level (L, M, Q, H):", QRMetadata.ValidLevelsRegExp);

// Assigning custom shapes to cells (optional)
QRRenderOptions renderOptions = new QRRenderOptions(CellColoringOptions.None);
renderOptions.Scale = 20;
renderOptions.SetDefaultCellShape(CellShape.Circle); // Note: The default shape for finder patterns is always CellShape.Square

// Creating the QR Code
QRCode qr = new(payload, errorCorrection, renderOptions);

// Rendering the image
Image<Rgb24> image = qr.RenderImage();

// Saving the image
image.Save("output.png");
```

You can also directly access internal structures like the version, blocks, or matrix for debugging/visualization.

---

## 🔒 Limitations

- Only Byte mode is supported right now.
- No structured append or Kanji encoding.
- No support for SVG output yet.

> Want to contribute? Feel free to fork and submit PRs!

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
