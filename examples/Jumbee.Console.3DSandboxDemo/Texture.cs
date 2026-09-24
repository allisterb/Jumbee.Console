namespace Jumbee.Console.SandboxDemo;

using System.Buffers.Binary;
using System.Numerics;
using System.Text;

using BitMiracle.LibJpeg.Classic;

using PngSharp.Api;
using PngSharp.Spec.Chunks.IHDR;

/// <summary>
/// An image map baked for the terminal: decoded once, box-reduced to the footprint a model can actually show, and
/// quantised onto a per-channel ramp. Sampled nearest, with no work left for the render loop.
/// </summary>
/// <remarks>
/// <para>
/// <b>Quantising is not optional.</b> The renderers' shade quantiser rounds the <em>lighting</em> and never touches a
/// texel's colour, so an image's arbitrary 24-bit colours would each reach the screen as a distinct colour — measured
/// on the capsule as 4.4× the untextured frame's ANSI bytes for a smooth gradient. Snapping to a ramp once, here,
/// costs nothing per frame. See <c>docs/internal/agent/3D Textures Plan.md</c>, Phase 1.
/// </para>
/// <para>
/// <b>Reducing is not optional either.</b> A model a hundred-odd sub-pixels across cannot show a 2048-texel map, and
/// nearest-sampling it unreduced turns detail into noise that shimmers as the model turns. The box reduce that fixes
/// that also yields <see cref="Retain"/> and <see cref="Contrast"/> for free, which is what lets a caller decline to
/// texture at all.
/// </para>
/// </remarks>
public sealed class Texture
{
    #region Constructors
    private Texture(byte[] rgb, int width, int height, int sourceWidth, int sourceHeight, float retain,
                    float contrast, int colours)
    {
        this.rgb = rgb;
        Width = width;
        Height = height;
        SourceWidth = sourceWidth;
        SourceHeight = sourceHeight;
        Retain = retain;
        Contrast = contrast;
        Colours = colours;
    }
    #endregion

    #region Properties
    /// <summary>Width of the baked map, in texels.</summary>
    public int Width { get; }

    /// <summary>Height of the baked map, in texels.</summary>
    public int Height { get; }

    /// <summary>Width of the decoded source image before the reduce.</summary>
    public int SourceWidth { get; }

    /// <summary>Height of the decoded source image before the reduce.</summary>
    public int SourceHeight { get; }

    /// <summary>How much of the source's colour variation survives the reduce: 1 when all of it does, towards 0 as
    /// more of it was detail finer than a baked texel.</summary>
    /// <remarks>A measure of spatial frequency against the baked resolution — not of how busy an image looks.
    /// <c>1 − RMS(pixel − its block's mean) / RMS(pixel − image mean)</c>.</remarks>
    public float Retain { get; }

    /// <summary>RMS distance of the baked texels from their mean colour, as a fraction of the largest possible
    /// (black to white). Near 0, the map survives but is nearly uniform.</summary>
    public float Contrast { get; }

    /// <summary>Distinct colours in the baked map, after quantisation — the figure that predicts the run cost.</summary>
    public int Colours { get; }

    /// <summary>The longest side a map is reduced to by default.</summary>
    /// <remarks>
    /// <para>
    /// Chosen by looking, on the capsule at 200×52: <b>64</b> merges its grid lines into blotches (1.81× the
    /// untextured bytes, and the messy outcome texturing exists to avoid); <b>128</b> keeps the grid but dims it
    /// (2.06×); <b>256</b> is visibly crisper for about 3% more (2.12×).
    /// </para>
    /// <para>
    /// The reason is geometric, and it generalises: a map wrapped <em>around</em> a model shows only about half its
    /// width on the visible side, so 256 texels put about 128 across a ~90-sub-pixel surface — close to 1:1. The
    /// earlier guess of 128 assumed the whole map is seen at once. Shimmer under rotation is unmeasured.
    /// </para>
    /// </remarks>
    public const int DefaultSize = 256;

    /// <summary>Levels per channel a map is quantised to by default.</summary>
    /// <remarks>Wolf3D's measured default, re-checked here on the capsule: 6 levels saves ~6% of bytes but
    /// posterises the hue into patchy steps; 16 costs ~3% more for nothing visible; off costs 2.33× against 2.06×.
    /// The saving is content-dependent — modest on line art over black, which coalesces into runs regardless, and
    /// large on a smooth gradient (4.4× → 1.77×), which is what a photograph is made of.</remarks>
    public const int DefaultLevels = 10;

    /// <summary>Largest source image accepted, in pixels. A header claiming more is refused before anything is
    /// allocated for it.</summary>
    public const long MaxSourcePixels = 64L * 1024 * 1024;
    #endregion

    #region Methods
    /// <summary>Loads and bakes a PNG or JPEG, told apart by the file's signature rather than its extension.</summary>
    /// <param name="path">The image to read.</param>
    /// <param name="size">The longest side of the baked map; a smaller source is not enlarged.</param>
    /// <param name="levels">Levels per channel to quantise to; 1 or less leaves colours unquantised.</param>
    /// <exception cref="InvalidDataException">The file is not a complete, valid PNG or JPEG. Every failure is
    /// reported as this one type, raised here rather than on first sample.</exception>
    public static Texture Load(string path, int size = DefaultSize, int levels = DefaultLevels) =>
        Decode(File.ReadAllBytes(path), size, levels, Path.GetFileName(path));

    /// <summary>The baked colour at a texture coordinate, nearest texel.</summary>
    /// <remarks>
    /// <para>
    /// <b>v is flipped</b>: OBJ puts <c>v = 0</c> at the bottom of the image, where a decoded image's first row is
    /// the top. Invisible with a procedural source, and a photograph upside down without it.
    /// </para>
    /// <para>
    /// Coordinates inside [0, 1] are <b>clamped</b>, outside it they <b>repeat</b>. Clamping matters at exactly 1.0,
    /// where an atlas puts its edge texels: wrapping would fetch the opposite edge and draw a sliver of the wrong
    /// colour along every island. Past 1 is how a tiling texture is authored, and there repeating is the meaning.
    /// </para>
    /// </remarks>
    public Color Sample(Vector2 uv)
    {
        var x = Math.Clamp((int)(Wrap(uv.X) * Width), 0, Width - 1);
        var y = Math.Clamp((int)((1f - Wrap(uv.Y)) * Height), 0, Height - 1);
        var i = ((y * Width) + x) * 3;
        return new Color(rgb[i], rgb[i + 1], rgb[i + 2]);
    }

    /// <summary>Decodes and bakes PNG or JPEG bytes. Split out so it can be exercised without a file.</summary>
    /// <exception cref="InvalidDataException">Every failure, whatever raised it.</exception>
    internal static Texture Decode(byte[] data, int size = DefaultSize, int levels = DefaultLevels, string name = "texture")
    {
        try
        {
            // By signature, not by extension: a map saved as .png that is really a JPEG still loads, and a file that
            // is neither gets a message that says so rather than a decoder error about the wrong format.
            var (rgb, width, height) =
                IsPng(data) ? DecodePng(data)
                : IsJpeg(data) ? DecodeJpeg(data)
                : throw new InvalidDataException("neither a PNG nor a JPEG: the file's signature is not recognised");
            return Bake(rgb, width, height, size, levels);
        }
        catch (Exception e)
        {
            // One type for every failure. PngSharp raises its own format and CRC exceptions, EndOfStream on a short
            // image stream, and KeyNotFound on a corrupt filter byte; libjpeg raises through StrictJpegErrors. A
            // caller should not need to know which.
            throw new InvalidDataException($"'{name}' is unreadable: {e.Message}", e);
        }
    }

    /// <summary>Bakes raw row-major RGB (top row first): box-reduce, measure, quantise.</summary>
    internal static Texture Bake(byte[] rgb, int width, int height, int size = DefaultSize, int levels = DefaultLevels)
    {
        var scale = Math.Min(1.0, (double)size / Math.Max(width, height));
        int bw = Math.Max(1, (int)Math.Round(width * scale)), bh = Math.Max(1, (int)Math.Round(height * scale));
        int Block(int x, int y) => (Math.Min(bh - 1, y * bh / height) * bw) + Math.Min(bw - 1, x * bw / width);

        // Pass 1: block sums and the image mean.
        var sum = new double[bw * bh * 3];
        var count = new int[bw * bh];
        double gr = 0, gg = 0, gb = 0;
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var b = Block(x, y);
                var i = ((y * width) + x) * 3;
                sum[b * 3] += rgb[i]; sum[(b * 3) + 1] += rgb[i + 1]; sum[(b * 3) + 2] += rgb[i + 2];
                gr += rgb[i]; gg += rgb[i + 1]; gb += rgb[i + 2];
                count[b]++;
            }

        var n = (double)width * height;
        (gr, gg, gb) = (gr / n, gg / n, gb / n);
        for (var b = 0; b < bw * bh; b++)
            for (var c = 0; c < 3; c++) sum[(b * 3) + c] = count[b] > 0 ? sum[(b * 3) + c] / count[b] : 0;

        // Pass 2: what the reduce destroys (each pixel against its block's mean) versus all the variation there was
        // (each pixel against the image mean). A uniform image has nothing to destroy, so it retains everything.
        double resid = 0, total = 0;
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var b = Block(x, y) * 3;
                var i = ((y * width) + x) * 3;
                resid += Sq(rgb[i] - sum[b]) + Sq(rgb[i + 1] - sum[b + 1]) + Sq(rgb[i + 2] - sum[b + 2]);
                total += Sq(rgb[i] - gr) + Sq(rgb[i + 1] - gg) + Sq(rgb[i + 2] - gb);
            }

        var retain = total <= 0 ? 1f : (float)(1.0 - Math.Sqrt(resid / total));

        // Contrast of what survived, measured before quantising so the ramp does not invent or erase any.
        double mr = 0, mg = 0, mb = 0;
        for (var b = 0; b < bw * bh; b++) { mr += sum[b * 3]; mg += sum[(b * 3) + 1]; mb += sum[(b * 3) + 2]; }
        (mr, mg, mb) = (mr / (bw * bh), mg / (bw * bh), mb / (bw * bh));
        double spread = 0;
        for (var b = 0; b < bw * bh; b++)
            spread += Sq(sum[b * 3] - mr) + Sq(sum[(b * 3) + 1] - mg) + Sq(sum[(b * 3) + 2] - mb);
        var contrast = (float)(Math.Sqrt(spread / (bw * bh)) / (255.0 * Math.Sqrt(3)));

        // Quantise onto the ramp, and count what is left.
        var baked = new byte[bw * bh * 3];
        var step = levels > 1 ? 255.0 / (levels - 1) : 0.0;
        var colours = new HashSet<int>();
        for (var b = 0; b < bw * bh; b++)
        {
            for (var c = 0; c < 3; c++)
            {
                var v = sum[(b * 3) + c];
                baked[(b * 3) + c] = (byte)Math.Clamp(Math.Round(step > 0 ? Math.Round(v / step) * step : v), 0, 255);
            }

            colours.Add((baked[b * 3] << 16) | (baked[(b * 3) + 1] << 8) | baked[(b * 3) + 2]);
        }

        return new Texture(baked, bw, bh, width, height, retain, contrast, colours.Count);
    }

    /// <summary>
    /// Refuses a PNG whose chunk structure is incomplete, before the decoder sees it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>This is what stands between a truncated file and a hung process.</b> PngSharp 0.6.0 reads chunk data in
    /// loops that subtract <c>Stream.Read</c>'s result without checking it for 0, so a file cut mid-chunk spins
    /// forever — measured on every truncation from 5% to 95% of a real PNG. A hang cannot be caught, and a texture
    /// loads before the UI exists. See the ledger in <c>reference/README.md</c>.
    /// </para>
    /// <para>
    /// Walking the chunks is sufficient: once every chunk is proven to lie inside the buffer and IEND is present,
    /// those loops can never reach the end of the stream. The decoder's <em>image</em> stream is already safe —
    /// its filter pass uses <c>ReadExactly</c>, which throws on short data. CRCs are left to PngSharp, which checks
    /// them and fails cleanly.
    /// </para>
    /// <para>
    /// The IHDR's dimensions are checked against <see cref="MaxSourcePixels"/> here too, so a header claiming an
    /// absurd size is a clear refusal rather than an out-of-memory crash inside the decoder.
    /// </para>
    /// </remarks>
    internal static void ValidatePngStructure(ReadOnlySpan<byte> png)
    {
        if (png.Length < 8 || !png[..8].SequenceEqual(Signature))
            throw new InvalidDataException("not a PNG: the signature is missing");

        long pos = 8;
        var first = true;
        while (true)
        {
            if (pos + 8 > png.Length)
                throw new InvalidDataException($"truncated: the file ends at byte {png.Length} with no IEND chunk");

            var length = BinaryPrimitives.ReadUInt32BigEndian(png[(int)pos..]);
            var type = Encoding.ASCII.GetString(png.Slice((int)pos + 4, 4));
            if (pos + 12 + length > png.Length)
                throw new InvalidDataException(
                    $"truncated: the '{type}' chunk at byte {pos} declares {length:N0} bytes, " +
                    $"but only {Math.Max(0, png.Length - pos - 8):N0} remain");

            if (first)
            {
                if (type != "IHDR" || length != 13) throw new InvalidDataException("the first chunk is not a 13-byte IHDR");
                long w = BinaryPrimitives.ReadUInt32BigEndian(png[(int)(pos + 8)..]);
                long h = BinaryPrimitives.ReadUInt32BigEndian(png[(int)(pos + 12)..]);
                if (w == 0 || h == 0 || w * h > MaxSourcePixels)
                    throw new InvalidDataException($"{w}x{h} is outside the {MaxSourcePixels:N0}-pixel limit");
                first = false;
            }

            if (type == "IEND") return;
            pos += 12 + length;
        }
    }
    #endregion

    #region Private methods
    // [0,1] inclusive clamps, anything else repeats -- see Sample.
    private static float Wrap(float t) => t is >= 0f and <= 1f ? t : t - MathF.Floor(t);

    private static double Sq(double d) => d * d;

    private static bool IsPng(ReadOnlySpan<byte> data) => data.Length >= 8 && data[..8].SequenceEqual(Signature);

    // Start of image, then the first marker's 0xFF.
    private static bool IsJpeg(ReadOnlySpan<byte> data) => data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF;

    private static (byte[] Rgb, int Width, int Height) DecodePng(byte[] png)
    {
        ValidatePngStructure(png);
        var image = Png.DecodeFromByteArray(png);
        return (PngToRgb(image), (int)image.Ihdr.Width, (int)image.Ihdr.Height);
    }

    // libjpeg.net, always under StrictJpegErrors: its default error manager decodes a truncated file silently and can
    // crash in its own recovery path (see that class). Output is always requested as RGB, which the port converts to
    // from greyscale, YCbCr and RGB sources; CMYK and YCCK are refused rather than guessed at.
    private static (byte[] Rgb, int Width, int Height) DecodeJpeg(byte[] jpeg)
    {
        var decoder = new jpeg_decompress_struct(new StrictJpegErrors());
        using var stream = new MemoryStream(jpeg, writable: false);
        decoder.jpeg_stdio_src(stream);
        decoder.jpeg_read_header(true);

        // The same cap as the PNG path, read from the header BEFORE start_decompress allocates anything for the image.
        if ((long)decoder.Image_width * decoder.Image_height > MaxSourcePixels)
            throw new InvalidDataException(
                $"{decoder.Image_width}x{decoder.Image_height} is outside the {MaxSourcePixels:N0}-pixel limit");

        decoder.Out_color_space = J_COLOR_SPACE.JCS_RGB;
        decoder.jpeg_start_decompress();
        int width = decoder.Output_width, height = decoder.Output_height;
        if (decoder.Output_components != 3)
            throw new InvalidDataException($"decoded to {decoder.Output_components} components where RGB was asked for");

        var rgb = new byte[width * height * 3];
        var row = jpeg_common_struct.AllocJpegSamples(width * 3, 1);
        while (decoder.Output_scanline < height)
        {
            var y = decoder.Output_scanline;
            // A memory source never suspends, so a short read here means something went wrong. Refuse rather than
            // hand back an image whose remaining rows were never written -- the silent partial image again.
            if (decoder.jpeg_read_scanlines(row, 1) != 1)
                throw new InvalidDataException($"the decoder stopped returning rows at {y} of {height}");
            Buffer.BlockCopy(row[0], 0, rgb, y * width * 3, width * 3);
        }

        decoder.jpeg_finish_decompress();
        return (rgb, width, height);
    }

    // PngSharp hands back PixelData in the file's own layout rather than normalised: palette indices for an indexed
    // image, several samples packed per byte below 8 bits, two bytes per sample at 16. Flattening that to RGB is
    // ours. Alpha and tRNS are dropped -- a diffuse map is opaque here -- and 16-bit keeps its high byte.
    private static byte[] PngToRgb(IRawPng png)
    {
        var h = png.Ihdr;
        int width = (int)h.Width, height = (int)h.Height;
        var channels = h.ColorType switch
        {
            ColorType.Grayscale or ColorType.IndexedColor => 1,
            ColorType.GrayscaleWithAlpha => 2,
            ColorType.TrueColor => 3,
            ColorType.TrueColorWithAlpha => 4,
            _ => throw new InvalidDataException($"unsupported colour type {h.ColorType}"),
        };

        var palette = png.Plte?.Entries;
        if (h.ColorType == ColorType.IndexedColor && palette is null)
            throw new InvalidDataException("an indexed-colour PNG with no PLTE chunk");

        // Below 8 bits samples are packed and rows are byte-aligned, so the stride is recomputed rather than assumed;
        // getting it wrong shears the image by a fraction of a pixel per row.
        var bits = h.BitDepth;
        var stride = ((width * channels * bits) + 7) / 8;
        var data = png.PixelData;
        if (data.Length < (long)stride * height)
            throw new InvalidDataException($"pixel data is {data.Length:N0} bytes, {(long)stride * height:N0} expected");

        var rgb = new byte[width * height * 3];
        Span<int> s = stackalloc int[4];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                for (var c = 0; c < channels; c++)
                {
                    var si = (x * channels) + c;
                    s[c] = bits switch
                    {
                        8 => data[(y * stride) + si],
                        16 => data[(y * stride) + (si * 2)],
                        _ => (data[(y * stride) + (si * bits / 8)] >> (8 - bits - ((si * bits) % 8))) & ((1 << bits) - 1),
                    };
                }

                var o = ((y * width) + x) * 3;
                switch (h.ColorType)
                {
                    case ColorType.IndexedColor:
                        var p = s[0] * 3;
                        if (p + 2 >= palette!.Length) throw new InvalidDataException($"palette index {s[0]} is out of range");
                        rgb[o] = palette[p]; rgb[o + 1] = palette[p + 1]; rgb[o + 2] = palette[p + 2];
                        break;
                    case ColorType.Grayscale:
                    case ColorType.GrayscaleWithAlpha:
                        // Sub-8-bit grey is stored at reduced range; scale it back up to 0..255.
                        var g = bits is 8 or 16 ? s[0] : s[0] * 255 / ((1 << bits) - 1);
                        rgb[o] = rgb[o + 1] = rgb[o + 2] = (byte)g;
                        break;
                    default:
                        rgb[o] = (byte)s[0]; rgb[o + 1] = (byte)s[1]; rgb[o + 2] = (byte)s[2];
                        break;
                }
            }
        }

        return rgb;
    }
    #endregion

    #region Fields
    private static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];
    private readonly byte[] rgb;
    #endregion

    #region Child types
    /// <summary>
    /// libjpeg's error manager with every <b>warning</b> made fatal and all output suppressed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>The default manager is unusable for untrusted files.</b> libjpeg treats a premature end of file as a warning:
    /// it inserts a fake end-of-image marker and carries on, so a truncated texture decodes "successfully" — measured
    /// at 87% of rows wrong for a file cut in half — and a file cut early crashes with an
    /// <see cref="IndexOutOfRangeException"/> inside the library's recovery path. Throwing at the first warning stops
    /// the decoder before it reaches either. See the ledger in <c>reference/README.md</c>.
    /// </para>
    /// <para>
    /// What it cannot catch is a property of the format: JPEG carries no checksums, so a flipped bit inside the
    /// compressed data decodes to a plausible-looking glitch with no warning at all.
    /// </para>
    /// </remarks>
    private sealed class StrictJpegErrors : jpeg_error_mgr
    {
        public override void output_message() { }

        // msg_level < 0 is a warning, which is fatal here; 0 and up are trace messages, which are dropped.
        public override void emit_message(int msg_level)
        {
            if (msg_level < 0) throw new InvalidDataException(format_message());
        }

        public override void error_exit() => throw new InvalidDataException(format_message());
    }
    #endregion
}
