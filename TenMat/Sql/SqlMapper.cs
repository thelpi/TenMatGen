using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;
using TenMat.Data;
using TenMat.Data.Enums;

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
        /// Loads players from the database by criteria.
        /// </summary>
        /// <param name="newPlayerAction">The action to execute with the loaded player.</param>
        /// <param name="youngerThan">Filters only players younger than the specified date.</param>
        /// <param name="sortByRankingAtDate">Applies a sort on the query based on the ranking at the specified date.</param>
        /// <exception cref="InvalidOperationException">An exception occured</exception>
        /// <exception cref="ArgumentNullException"><paramref name="newPlayerAction"/> is <c>Null</c>.</exception>
        public void LoadPlayers(Action<Player> newPlayerAction, DateTime? youngerThan, DateTime? sortByRankingAtDate)
        {
            if (newPlayerAction == null)
            {
                throw new ArgumentNullException(nameof(newPlayerAction));
            }

            GenericLoad((command) =>
            {
                var sqlBuilder = new StringBuilder();
                sqlBuilder.AppendLine("SELECT id, first_name, last_name, birth_date FROM player");
                if (youngerThan.HasValue)
                {
                    sqlBuilder.AppendLine("WHERE birth_date >= @dob");
                    command.AddDateTimeParameter("@dob", youngerThan.Value);
                }
                if (sortByRankingAtDate.HasValue)
                {
                    sqlBuilder.AppendLine("ORDER BY IFNULL((");
                    sqlBuilder.AppendLine(" SELECT r.ranking FROM ranking AS r");
                    sqlBuilder.AppendLine(" WHERE r.player_id = player.id");
                    sqlBuilder.AppendLine(" AND r.version_id = 2");
                    sqlBuilder.AppendLine(" AND r.date >= @rank_date");
                    sqlBuilder.AppendLine(" ORDER BY r.date ASC");
                    sqlBuilder.AppendLine(" LIMIT 0,1");
                    sqlBuilder.AppendLine("), 9999) ASC");
                    command.AddDateTimeParameter("@rank_date", sortByRankingAtDate.Value);
                }
                command.CommandText = sqlBuilder.ToString();
            }, 
            (reader) =>
            {
                newPlayerAction(new Player(reader.Get<UInt32>("id"),
                    reader.Get<string>("first_name"),
                    reader.Get<string>("last_name"),
                    reader.Get<DateTime?>("birth_date")));
            });
        }

        /// <summary>
        /// Loads every matches of the specified player.
        /// Walkover matches are excluded.
        /// </summary>
        /// <param name="player">Instance of <see cref="Player"/>.</param>
        /// <param name="afterThat">Filters to matches player after this date (or the same day).</param>
        /// <param name="dontReload">Doesn't load matches from database if <see cref="Player.MatchHistorySet"/> is <c>True</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="player"/> is <c>Null</c>.</exception>
        public void LoadMatches(Player player, DateTime? afterThat, bool dontReload)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (dontReload && player.MatchHistorySet)
            {
                return;
            }

            List<MatchArchive> matches = new List<MatchArchive>();
            GenericLoad((command) =>
            {
                var sbSql = new StringBuilder();
                sbSql.AppendLine("SELECT best_of, winner_id, loser_id, round_id,");
                sbSql.AppendLine("edition.date_begin, edition.level_id, edition.surface_id,");
                sbSql.AppendLine("w_set_1, l_set_1, tb_set_1, w_set_2, l_set_2, tb_set_2,");
                sbSql.AppendLine("w_set_3, l_set_3, tb_set_3, w_set_4, l_set_4, tb_set_4,");
                sbSql.AppendLine("w_set_5, l_set_5, tb_set_5, w_sv_gms, l_sv_gms");
                sbSql.AppendLine("FROM match_general AS mg");
                sbSql.AppendLine("INNER JOIN edition ON edition_id = edition.id");
                sbSql.AppendLine("INNER JOIN match_score AS ms ON mg.id = ms.match_id");
                sbSql.AppendLine("INNER JOIN match_stat AS mt ON mg.id = mt.match_id");
                sbSql.AppendLine("WHERE (winner_id = @pid OR loser_id = @pid) AND walkover = 0 ");
                if (afterThat.HasValue)
                {
                    sbSql.AppendLine("AND edition.date_begin >= @date");
                    command.AddDateTimeParameter("@date", afterThat.Value);
                }
                command.CommandText = sbSql.ToString();
                command.AddParameter("@pid", player.Id, DbType.UInt32);
            }, (reader) =>
            {
                var sets = new List<Tuple<uint, uint, uint?>>();
                for (int i = 1; i <= 5; i++)
                {
                    if (!reader.IsDBNull($"w_set_{i}") || !reader.IsDBNull($"w_set_{i}"))
                    {
                        sets.Add(new Tuple<uint, uint, uint?>(
                            reader.Get<uint?>($"w_set_{i}").GetValueOrDefault(0),
                            reader.Get<uint?>($"l_set_{i}").GetValueOrDefault(0),
                            reader.Get<uint?>($"tb_set_{i}")
                        ));
                    }
                    else
                    {
                        break;
                    }
                }

                matches.Add(new MatchArchive(reader.Get<SurfaceEnum>("surface_id"),
                    reader.Get<LevelEnum>("level_id"),
                    reader.Get<RoundEnum>("round_id"),
                    (BestOfEnum)reader.Get<uint>("best_of"),
                    reader.Get<DateTime>("date_begin"),
                    reader.Get<uint>("winner_id"),
                    reader.Get<uint>("loser_id"),
                    sets,
                    reader.Get<uint?>("w_sv_gms"),
                    reader.Get<uint?>("l_sv_gms")));
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
