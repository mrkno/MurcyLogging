﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Agilefantasy.Common;
using Newtonsoft.Json;

namespace Agilefantasy
{
    public class AgilefantEffortEntry :  AgilefantBase
    {
        [JsonProperty("date")]
        private long LogTimeMilliseconds { get; set; }

        public DateTime LogDate { get { return new DateTime(1970, 1, 1).AddMilliseconds(LogTimeMilliseconds); } }

        [JsonProperty("description")]
        public string Comment { get; private set; }

        [JsonProperty("minutesSpent")]
        public int MinutesSpent { get; private set; }
        
        [JsonProperty("user")]
        public AgilefantUser User { get; private set; }

        /// <summary>
        /// Gets all the effort entries for a backlog
        /// </summary>
        /// <param name="from">The backlog item to get the effort entries for</param>
        /// <param name="session">The session</param>
        /// <returns>The effort entries</returns>
        internal static Task<IEnumerable<AgilefantEffortEntry>> GetEffortEntries(IAgilefantLoggable from,
            AgilefantSession session)
        {
            return GetEffortEntries(from.Id, session);
        }

        /// <summary>
        /// Gets all the effort entries for a backlog
        /// </summary>
        /// <param name="from">The backlog item to get the effort entries for</param>
        /// <param name="session">The session</param>
        /// <returns>The effort entries</returns>
        internal static async Task<IEnumerable<AgilefantEffortEntry>> GetEffortEntries(int from, AgilefantSession session)
        {
            var query = string.Format("ajax/retrieveTaskHourEntries.action?parentObjectId={0}&limited=false", from);
            var response = await session.Get(query);
            
            var json = await response.Content.ReadAsStringAsync();
            var entries = JsonConvert.DeserializeObject <AgilefantEffortEntry[]>(json);
            return entries;
        }

        /// <summary>
        /// Logs time against an backlog item
        /// </summary>
        /// <param name="against">The item to log against</param>
        /// <param name="entryDate">The entry date</param>
        /// <param name="minutesSpent">The minutes spent</param>
        /// <param name="description">A description of the work done</param>
        /// <param name="users">The users to log time for</param>
        /// <param name="session">The session</param>
        internal static Task LogTime(IAgilefantLoggable against, DateTime entryDate, int minutesSpent,
            string description,
            IEnumerable<AgilefantUser> users, AgilefantSession session)
        {
            return LogTime(against.Id, entryDate, minutesSpent, description, from user in users select user.Id, session);
        }

        /// <summary>
        /// Adds an effort entry to the specified loggable
        /// </summary>
        /// <param name="parentObjectId">The id of the object to log against</param>
        /// <param name="entryDate">The date of the entry</param>
        /// <param name="minutesSpent">The time spent, in minutes</param>
        /// <param name="description">A description of the entry</param>
        /// <param name="users">The users to log against</param>
        /// <param name="session">The session</param>
        internal static Task LogTime(int parentObjectId, DateTime entryDate, int minutesSpent, string description,
            IEnumerable<int> users, AgilefantSession session)
        {
            //Get the time in milliseconds since the epoch
            var timeSinceEpoch = (long)(entryDate - new DateTime(1970, 1, 1)).TotalMilliseconds;

            //Get a comma seperated string of the users
            var userIds = "";
            foreach (var user in users)
            {
                if (!string.IsNullOrEmpty(userIds))
                    userIds += ",";
                userIds += user;
            }

            var postData = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"parentObjectId", parentObjectId.ToString()},
                {"hourEntry.date", timeSinceEpoch.ToString()},
                {"hourEntry.minutesSpent", minutesSpent.ToString()},
                {"hourEntry.description", description},
                {"userIds", userIds},
            });

            return session.Post("ajax/logTaskEffort.action", postData);
        }

        /// <summary>
        /// Updates an existing effort entry
        /// </summary>
        /// <param name="entry">The effort entry to update. This *MUST* have the correct id</param>
        /// <param name="session">The session</param>
        internal static Task UpdateEffortEntry(AgilefantEffortEntry entry, AgilefantSession session)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"hourEntry.minutesSpent", entry.MinutesSpent.ToString()},
                {"hourEntry.date", entry.LogTimeMilliseconds.ToString()},
                {"hourEntry.description", entry.Comment},
                {"hourEntryId", entry.Id.ToString() },
            });

            return session.Post("ajax/storeEffortEntry.action", content);
        }
    }
}
