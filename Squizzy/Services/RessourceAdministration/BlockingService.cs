using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squizzy.Extensions;

namespace Squizzy.Services
{
    public class BlockingService : SquizzyService
    {
        public int BlockingCount => BlockedUsers.Count + BlockedChannels.Count + BlockedGuilds.Count + BlockedGlobal.Count;

        private Dictionary<ulong, List<ulong>> BlockedUsers { get; set; }
        private object UserLock { get; set; }
        private Dictionary<ulong, List<ulong>> BlockedChannels { get; set; }
        private object ChannelLock { get; set; }
        private Dictionary<ulong, List<ulong>> BlockedGuilds { get; set; }
        private object GuildLock { get; set; }
        private Dictionary<ulong, object> BlockedGlobal { get; set; }
        private object GlobalLock { get; set; }

        public override Task InitializeAsync()
        {
            BlockedUsers = new Dictionary<ulong, List<ulong>>();
            UserLock = new object();

            BlockedChannels = new Dictionary<ulong, List<ulong>>();
            ChannelLock = new object();

            BlockedGuilds = new Dictionary<ulong, List<ulong>>();
            GuildLock = new object();

            BlockedGlobal = new Dictionary<ulong, object>();
            GlobalLock = new object();

            return base.InitializeAsync();
        }

        //The Locks are needed because IsxxxBlocked Enumerates through the Dictionary which requires it to be locked.
        //Uses normal Dictionary because the blockins are needed anyway and the Concurrent One is less performant

        #region Block
        public void BlockUser(ulong commandId, ulong userId)
        {
            lock (UserLock)
            {
                BlockedUsers.AddOrUpdate(commandId,
               id => new List<ulong>() { userId },
               (id, list) => { list.Add(userId); return list; });
            }
        }

        public void BlockUsers(ulong commandId, params ulong[] userIds)
        {
            lock (UserLock)
            {
                BlockedUsers.AddOrUpdate(commandId,
                           id => new List<ulong>(userIds),
                           (id, list) => { list.AddRange(userIds); return list; });
            }
        }

        public void BlockChannel(ulong commandId, ulong channelId)
        {
            lock (ChannelLock)
            {
                BlockedChannels.AddOrUpdate(commandId,
                           id => new List<ulong>() { channelId },
                           (id, list) => { list.Add(channelId); return list; });
            }
        }

        public void BlockGuild(ulong commandId, ulong guildId)
        {
            lock (GuildLock)
            {
                BlockedGuilds.AddOrUpdate(commandId,
                           id => new List<ulong>() { guildId },
                           (id, list) => { list.Add(guildId); return list; });
            }
        }

        public void BlockGlobal(ulong commandId)
        {
            lock(GlobalLock)
            {
                BlockedGlobal.AddOrUpdate(commandId,
                           id => null,
                           (id, obj) => null);
            }
        }
        #endregion
    
        #region Unblock
        public void UnblockCommandId(ulong commandId)
        {
            lock (UserLock)
            {
                BlockedUsers.Remove(commandId);
            }
            lock (ChannelLock)
            {
                BlockedChannels.Remove(commandId);
            }
            lock (GuildLock)
            {
                BlockedGuilds.Remove(commandId);
            }
            lock (GlobalLock)
            {
                BlockedGlobal.Remove(commandId);
            }
        }

        public void UnblockUser(ulong commandId, ulong userId)
        {
            lock(UserLock)
            {
                BlockedUsers.TryUpdate(commandId,
                    (key, list) => { list.Remove(userId); return list; });
            }
        }

        public void UnblockChannel(ulong commandId, ulong channelId)
        {
            lock (UserLock)
            {
                BlockedChannels.TryUpdate(commandId,
                    (key, list) => { list.Remove(channelId); return list; });
            }
        }
        public void UnblockGuild(ulong commandId, ulong guildId)
        {
            lock (UserLock)
            {
                BlockedGuilds.TryUpdate(commandId,
                    (key, list) => { list.Remove(guildId); return list; });
            }
        }

        public void UnblockGlobal(ulong commandId)
        {
            lock (UserLock)
            {
                BlockedGlobal.Remove(commandId);
            }
        }
        #endregion

        #region IsBlocked
        public bool IsUserBlocked(ulong userId)
        {
            lock(UserLock)
            {
                return BlockedUsers.Values.Any(x => x.Contains(userId));
            }
        }

        public bool IsChannelBlocked(ulong channelId)
        {
            lock (ChannelLock)
            {
                return BlockedChannels.Values.Any(x => x.Contains(channelId));
            }
        }

        public bool IsGuildBlocked(ulong guildId)
        {
            lock (GuildLock)
            {
                return BlockedGuilds.Values.Any(x => x.Contains(guildId));
            }
        }

        public bool IsGlobalBlocked()
        {
            lock (GlobalLock)
            {
                return BlockedGlobal.Any();
            }
        }
        #endregion
    }
}
