using VkLib;

namespace Colibri.Services
{
    public class ServiceLocator
    {
        private const string VKAPP_ID = "3059212";
        private const string VKAPP_SECRET = "CEPu3hrA9DK5Ur6pBbia";

        public static Vk Vkontakte { get; } = new Vk(VKAPP_ID, VKAPP_SECRET, "5.38");

        public static LongPollHandler LongPollHandler { get; } = new LongPollHandler(Vkontakte);

        public static UserService UserService { get; } = new UserService(Vkontakte);

        public static ExceptionHandler ExceptionHandler { get; } = new ExceptionHandler();

        public static AudioService AudioService { get; } = new AudioService();

        public static UserPresenceService UserPresenceService { get; } = new UserPresenceService(Vkontakte);
    }
}