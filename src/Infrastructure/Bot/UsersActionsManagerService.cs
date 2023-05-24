namespace PiVPNManager.Infrastructure.Bot
{
    public sealed class UsersActionsManagerService
    {
        private readonly Dictionary<long, UserClient> _creatingClients = new Dictionary<long, UserClient>();

        public void AddClientServer(long userId, int serverId)
        {
            if (!_creatingClients.ContainsKey(userId))
            {
                _creatingClients.Add(userId, new UserClient());
            }

            _creatingClients[userId].ServerId = serverId;
        }

        public void AddClientName(long userId, string clientName)
        {
            if (!_creatingClients.ContainsKey(userId))
            {
                _creatingClients.Add(userId, new UserClient());
            }

            _creatingClients[userId].ClientName = clientName;
        }

        public UserClient GetUserClient(long userId)
        {
            if (!_creatingClients.ContainsKey(userId))
            {
                throw new ArgumentException();
            }

            return _creatingClients[userId];
        }

        public void RemoveUserClient(long userId)
        {
            if (!_creatingClients.ContainsKey(userId))
            {
                throw new ArgumentException();
            }

            _creatingClients.Remove(userId);
        }
    }
}
