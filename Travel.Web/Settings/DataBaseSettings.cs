using MongoDB.Driver;

namespace Travel.Web.Settings
{
    public class DataBaseSettings : IDataBaseSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DataBaseName { get; set; } = string.Empty;
        public string TourCollectionName { get; set; } = "Tours";
        public string DestinationCollectionName { get; set; } = "Destinations";
        public string CategoryCollectionName { get; set; } = "Categories";
        public string ReservationCollectionName { get; set; } = "Reservations";
        public string CommentCollectionName { get; set; } = "Comments";
        public string QuestionCollectionName { get; set; } = "Questions";
        public string FavoriteCollectionName { get; set; } = "Favorites";
        public string BannerCollectionName { get; set; } = "Banners";
    }
}