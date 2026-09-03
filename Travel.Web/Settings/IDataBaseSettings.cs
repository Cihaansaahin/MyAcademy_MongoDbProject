using MongoDB.Driver;

namespace Travel.Web.Settings
{
    public interface IDataBaseSettings
    {
        string ConnectionString { get; set; }
        string DataBaseName { get; set; }
        string TourCollectionName { get; set; }
        string DestinationCollectionName { get; set; }
        string CategoryCollectionName { get; set; }
        string ReservationCollectionName { get; set; }
        string CommentCollectionName { get; set; }
        string QuestionCollectionName { get; set; }
        string FavoriteCollectionName { get; set; }
        string BannerCollectionName { get; set; }
    }
}