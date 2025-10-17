namespace SignaturePad.Models
{
    public class Signature
    {
        public Guid Id { get; set; } = new Guid();
        public byte[]? ImageData { get; set; }
    }
}
