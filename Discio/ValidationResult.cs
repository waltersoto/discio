

namespace Discio
{
    public struct ValidationResult
    {
        public bool Folder { get; set; }
        public bool Master { get; set; }

        public bool IsValid => Folder && Master;


    }
}
