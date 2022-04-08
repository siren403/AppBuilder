namespace AppBuilder
{
    public readonly struct iOSConfigureBuilder
    {
        private readonly BuildConfigureRecorder _recorder;

        public iOSConfigureBuilder(BuildConfigureRecorder recorder)
        {
            _recorder = recorder;
        }

    }
}