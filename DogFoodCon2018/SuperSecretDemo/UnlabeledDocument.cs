namespace BTJ.SPO.AutoApplyRetentionLabel
{
    public class UnlabeledDocument
    {
        private string _WebUrl;
        private string _DocPath;
        private string _RetentionLabel;

        public UnlabeledDocument(string webUrl, string docPath, string retentionLabel)
        {
            WebUrl = webUrl;
            DocPath = docPath;
            RetentionLabel = retentionLabel;
        }

        public string WebUrl { get => _WebUrl; set => _WebUrl = value; }
        public string DocPath { get => _DocPath; set => _DocPath = value; }
        public string RetentionLabel { get => _RetentionLabel; set => _RetentionLabel = value; }
    }
}
