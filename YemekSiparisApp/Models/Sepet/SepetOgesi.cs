namespace YemekSiparisApp.Models.Sepet
{
    public class SepetOgesi
    {
        public int MenuKalemiId { get; set; }
        public string UrunAdi { get; set; } = string.Empty;
        public decimal BirimFiyat { get; set; }
        public int Miktar { get; set; }
        
        // Sepetteki aynı ürünü temsil eden restoran(Eğer sepete başka bir restorandan eklenirse uyarı vermek için)
        public int RestoranId { get; set; }
        public string RestoranAdi { get; set; } = string.Empty;

        public decimal ToplamTutar => BirimFiyat * Miktar;
    }
}
