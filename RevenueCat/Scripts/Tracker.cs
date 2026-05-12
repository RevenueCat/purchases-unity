namespace RevenueCat.AdTracking
{
    public class Tracker
    {
        private readonly IPurchasesWrapper _wrapper;

        internal Tracker(IPurchasesWrapper wrapper)
        {
            _wrapper = wrapper;
        }

        public void TrackAdDisplayed(AdDisplayedData data) => _wrapper.TrackAdDisplayed(data);
        public void TrackAdOpened(AdOpenedData data) => _wrapper.TrackAdOpened(data);
        public void TrackAdRevenue(AdRevenueData data) => _wrapper.TrackAdRevenue(data);
        public void TrackAdLoaded(AdLoadedData data) => _wrapper.TrackAdLoaded(data);
        public void TrackAdFailedToLoad(AdFailedToLoadData data) => _wrapper.TrackAdFailedToLoad(data);
    }
}
