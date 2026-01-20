//// In Digital Activities aggregate/command -  Handler
//using Cannery.Aspects.Domain;

//public class FeatureEnrolledEventHandler : IDomainEventHandler<FeatureEnrolledEvent>
//{
//    private readonly IDigitalActivityService _activityService;

//    public FeatureEnrolledEventHandler(IDigitalActivityService activityService)
//    {
//        _activityService = activityService;
//    }

//    public async Task HandleAsync(FeatureEnrolledEvent domainEvent)
//    {
//        await _activityService.CreateUnconfiguredActivities(domainEvent.AssetId, domainEvent.FeatureType);
//    }
//}

//// In Digital Assets aggregate/command - Dispatcher
//public class DigitalAssetService
//{
//    private readonly IRepository<DigitalAsset> _repository;
//    private readonly IDomainEventDispatcher _domainEventDispatcher;
//    public DigitalAssetService(IRepository<DigitalAsset> repository, IDomainEventDispatcher domainEventDispatcher)
//    {
//        _repository = repository;
//        _domainEventDispatcher = domainEventDispatcher;
//    }
//    public async Task EnrollFeatureAsync(Guid digitalAssetId, string featureType)
//    {
//        var digitalAsset = await _repository.GetAsync(digitalAssetId);
//        digitalAsset.EnrollFeature(featureType);

//        await _repository.SaveAsync(digitalAsset);

//        await _domainEventDispatcher.DispatchAsync(digitalAsset.DomainEvents);
//        digitalAsset.ClearDomainEvents();
//    }
//}