using LocationApp.services;
using Topshelf;

class Program
{
    static void Main(string[] args)
    {
        HostFactory.Run(x =>
        {
            x.Service<LocationService>(s =>
            {
                s.ConstructUsing(name => new LocationService());
                s.WhenStarted(service => service.Start());
                s.WhenStopped(service => service.Stop());
            });

            x.RunAsLocalSystem();
            x.StartAutomatically();
            x.SetDescription("IP Location App");
            x.SetDisplayName("IpAkhilaLocationApp");
            x.SetServiceName("IpAkhilaLocationApp");
        });
    }
}