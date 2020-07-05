using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using TenMat.Data;

namespace TenMat.Sql
{
    /// <summary>
    /// Sql mapper.
    /// </summary>
    public class SqlMapper
    {
        private const string ConnectionStringPattern = "Server={0};Database={1};Uid={2};Pwd={3};";

        private readonly Func<IDbConnection> _getConnection;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="server">Database server.</param>
        /// <param name="database">Database name.</param>
        /// <param name="user">Database user.</param>
        /// <param name="password">Database user password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="server"/> is <c>Null</c>, empty or white spaces only.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="database"/> is <c>Null</c>, empty or white spaces only.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>Null</c>, empty or white spaces only.</exception>
        /// <exception cref="InvalidOperationException">The database connection is invalid.</exception>
        public SqlMapper(string server, string database, string user, string password)
        {
            if (string.IsNullOrWhiteSpace(server))
            {
                throw new ArgumentNullException(nameof(server));
            }

            if (string.IsNullOrWhiteSpace(database))
            {
                throw new ArgumentNullException(nameof(database));
            }

            if (string.IsNullOrWhiteSpace(user))
            {
                throw new ArgumentNullException(nameof(user));
            }

            _getConnection = () => new MySqlConnection(
                string.Format(ConnectionStringPattern, 
                    server.Trim(), database.Trim(), user.Trim(), (password ?? "").Trim()));

            string tryConnectionResult = TryConnection();
            if (!string.IsNullOrWhiteSpace(tryConnectionResult))
            {
                throw new InvalidOperationException($"The database connection is invalid. Internal error: \"{tryConnectionResult}\"");
            }
        }

        private string TryConnection()
        {
            string result = string.Empty;

            IDbConnection connection = null;
            try
            {
                using (connection = _getConnection())
                {
                    connection.Open();
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            finally
            {
                connection?.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Loads every players from the database into <see cref="Player.Instances"/>.
        /// </summary>
        /// <param name="youngerThan">Filters only players younger than the specified date.</param>
        /// <exception cref="InvalidOperationException">An exception occured</exception>
        public void LoadPlayers(DateTime? youngerThan)
        {
            GenericLoad((command) =>
            {
                command.CommandText = "SELECT id, first_name, last_name, birth_date FROM player";
                if (youngerThan.HasValue)
                {
                    command.CommandText += " WHERE birth_date >= @dob";
                    command.AddDateTimeParameter("@dob", youngerThan.Value);
                }
            }, 
            (reader) =>
            {
                Player.AddPlayer(new Player
                {
                    DateOfBirth = reader.Get<DateTime?>("birth_date"),
                    Id = reader.Get<UInt32>("id"),
                    Name = Player.GetFullName(reader.Get<string>("first_name"), reader.Get<string>("last_name"))
                });
            });
        }

        /// <summary>
        /// Loads every matches of the specified player.
        /// Walkover matches are excluded.
        /// </summary>
        /// <param name="player">Instance of <see cref="player"/>.</param>
        /// <param name="afterThat">Filters to matches player after this date (or the same day).</param>
        /// <exception cref="ArgumentNullException"><paramref name="player"/> is <c>Null</c>.</exception>
        public void LoadMatches(Player player, DateTime? afterThat)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            List<MatchHistory> matches = new List<MatchHistory>();
            GenericLoad((command) =>
            {
                command.CommandText = "SELECT best_of, winner_id, loser_id, round_id, " +
                "edition.date_begin, edition.level_id, edition.surface_id " +
                "FROM match_general INNER JOIN edition ON edition_id = edition.id " +
                "WHERE (winner_id = @pid OR loser_id = @pid) AND walkover = 0 ";
                if (afterThat.HasValue)
                {
                    command.CommandText += "AND edition.date_begin >= @date";
                    command.AddDateTimeParameter("@date", afterThat.Value);
                }
                command.AddParameter("@pid", player.Id, DbType.UInt32);
            }, (reader) =>
            {
                matches.Add(new MatchHistory
                {
                    BestOf = reader.Get<uint>("best_of"),
                    Date = reader.Get<DateTime>("date_begin"),
                    Level = reader.Get<LevelEnum>("level_id"),
                    LoserId = reader.Get<uint>("loser_id"),
                    Round = reader.Get<RoundEnum>("round_id"),
                    Surface = reader.Get<SurfaceEnum>("surface_id"),
                    WinnerId = reader.Get<uint>("winner_id"),
                });
            });
            player.SetMatchHistoryList(matches);
        }

        private void GenericLoad(Action<IDbCommand> PrepareCommand, Action<IDataReader> ReaderAction)
        {
            IDbConnection connection = null;
            IDbCommand command = null;
            IDataReader reader = null;
            try
            {
                using (connection = _getConnection())
                {
                    connection.Open();
                    using (command = connection.CreateCommand())
                    {
                        PrepareCommand(command);
                        using (reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ReaderAction(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An exception occured. See {nameof(Exception.InnerException)} for more details.", ex);
            }
            finally
            {
                reader?.Dispose();
                command?.Dispose();
                connection?.Dispose();
            }
        }
    }
}
