namespace Travel.Web.Entities.Common
{
    public class LocalizedString
    {
        public string Tr { get; set; } = string.Empty;
        public string En { get; set; } = string.Empty;

        // Geçerli thread kültürüne göre metni döner; biri boşsa diğer dili gösterir
        public string Value
        {
            get
            {
                var isEn = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower() == "en";
                if (isEn)
                {
                    return !string.IsNullOrWhiteSpace(En) ? En : Tr;
                }
                return !string.IsNullOrWhiteSpace(Tr) ? Tr : En;
            }
        }

        public override string ToString() => Value;
    }
}