using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data;
using System.Diagnostics;

namespace TheBagBunker
{
    public class SharedLibrary
    {
        public static int _lockIdleInterval = 60; // in seconds
        public static string _ConnectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"]
            .ConnectionString;

        public int IsLockerAvaiable(MySqlConnection conn)
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                string query = @"SELECT Id
                                FROM Lockers
                                WHERE IsLockerInUse = 0 AND IsLockerInProcess = 0
                                ORDER BY RowNumber DESC, ColumnNumber ASC
                                LIMIT 1;";

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    var result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result); // Return the Locker ID
                    }
                }
                conn.Close();
                return 0; // No locker available
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }

            return 0; // Return 0 if an error occurs
        }

        public bool FreeTheLockerAndUser(int userId, int lockerId, decimal secondAmount, string secondCurrency)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_ConnectionString))
                {
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();

                    DataTable userTable = new DataTable();
                    DataTable lockerInfoTable = new DataTable();

                    string query = @"SELECT * FROM Users WHERE Id = @UserId;";

                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = query;
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        using (MySqlDataAdapter SDA = new MySqlDataAdapter(cmd))
                        {
                            SDA.Fill(userTable);
                        }

                        query = "SELECT * FROM LockerInformation WHERE UserId = @UserId AND LockerId = @LockerId;";
                        cmd.CommandText = query;
                        cmd.Parameters.AddWithValue("@LockerId", lockerId);

                        using (MySqlDataAdapter SDA = new MySqlDataAdapter(cmd))
                        {
                            SDA.Fill(lockerInfoTable);
                        }

                        cmd.Parameters.Clear();
                        cmd.CommandText = @"INSERT INTO UsersBackup (Email, FullName, MainId, Nationality, PassportNo,
                                            Password, Phone, CreatedOn)
                                            VALUES (@Email, @FullName, @MainId1, @Nationality, @PassportNo, @Password,
                                            @Phone, @CreatedOn1);";

                        cmd.Parameters.AddWithValue("@Email", userTable.Rows[0]["Email"].ToString());
                        cmd.Parameters.AddWithValue("@FullName", userTable.Rows[0]["FullName"].ToString());
                        cmd.Parameters.AddWithValue("@MainId1", Convert.ToInt32(userTable.Rows[0]["Id"]));
                        cmd.Parameters.AddWithValue("@Nationality", userTable.Rows[0]["Nationality"].ToString());
                        cmd.Parameters.AddWithValue("@PassportNo", userTable.Rows[0]["PassportNo"].ToString());
                        cmd.Parameters.AddWithValue("@Password", userTable.Rows[0]["Password"].ToString());
                        cmd.Parameters.AddWithValue("@Phone", userTable.Rows[0]["Phone"].ToString());
                        cmd.Parameters.AddWithValue("@CreatedOn1", DateTime.UtcNow);
                        //cmd.ExecuteNonQuery();

                        //cmd.Parameters.Clear();
                        cmd.CommandText += @"INSERT INTO LockerInformationBackup (MainId, FirstAmount, FirstCurrency,
                                            SecondAmount, SecondCurrency, UserId, LockerId, LockerUpTime, CreatedOn)
                                            VALUES (@MainId2, @FirstAmount, @FirstCurrency, @SecondAmount, @SecondCurrency,
                                                @UserId1, @LockerId1, @LockerUpTime, @CreatedOn2);";
                        cmd.Parameters.AddWithValue("@MainId2", lockerInfoTable.Rows[0]["Id"].ToString());
                        cmd.Parameters.AddWithValue("@FirstAmount", Convert.ToDecimal(lockerInfoTable.Rows[0]["Amount"]));
                        cmd.Parameters.AddWithValue("@FirstCurrency", lockerInfoTable.Rows[0]["Currency"].ToString());
                        cmd.Parameters.AddWithValue("@SecondAmount", secondAmount);
                        cmd.Parameters.AddWithValue("@SecondCurrency", secondCurrency);
                        cmd.Parameters.AddWithValue("@UserId1", userId);
                        cmd.Parameters.AddWithValue("@LockerId1", lockerId);
                        cmd.Parameters.AddWithValue("@LockerUpTime", Convert.ToDateTime(lockerInfoTable.Rows[0]["LockerUpTime"]));
                        cmd.Parameters.AddWithValue("@CreatedOn2", DateTime.UtcNow);
                        //cmd.ExecuteNonQuery();

                        //cmd.Parameters.Clear();
                        cmd.CommandText += @"DELETE FROM LockerInformation WHERE UserId = @UserId2 AND LockerId = @LockerId2;
                                     UPDATE Lockers SET IsLockerInUse = 0 WHERE Id = @LockerId2;
                                     DELETE FROM Users WHERE Id = @UserId2;";
                        cmd.Parameters.AddWithValue("@UserId2", userId);
                        cmd.Parameters.AddWithValue("@LockerId2", lockerId);
                        cmd.ExecuteNonQuery();
                    }

                    conn.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
            return false;
        }

        public (int lockerId, DateTime processedTime) LockerAvailability(MySqlConnection conn, int userId)
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "CALL LockerAvailability(@UserId);";
                    cmd.Parameters.AddWithValue("UserId", userId);

                    MySqlDataAdapter SDA = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    SDA.Fill(dt);
                    conn.Close();

                    int lockerId = Convert.ToInt32(dt.Rows[0]["LockerId"]);

                    if (lockerId == 0)
                        return (0, DateTime.MinValue); // No locker Available

                    return (Convert.ToInt32(dt.Rows[0]["LockerId"]), Convert.ToDateTime(dt.Rows[0]["ProcessedTime"]));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }

            return (0, DateTime.MinValue); // Return 0 if an error occurs
        }

        public void ReleaseLockOnLocker(int lockerId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_ConnectionString))
                {
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();
                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE Lockers " +
                            "SET IsLockerInProcess = 0, LockerInProcessTime = NULL " +
                            "WHERE Id = @LockerId AND IsLockerInProcess = 1 " +
                            "AND TIMESTAMPDIFF(SECOND, LockerInProcessTime, NOW()) > @TimeInterval;" +
                            "" +
                            "DELETE FROM TimeReservedLocks WHERE LockerId = @LockerId;";
                        cmd.Parameters.AddWithValue("@TimeInterval", _lockIdleInterval);
                        cmd.Parameters.AddWithValue("@LockerId", lockerId);
                        cmd.ExecuteNonQuery();
                    }
                    conn.Clone();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public void ReleaseIdleLocks()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_ConnectionString))
                {
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();
                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"CREATE TEMPORARY TABLE LockerIDs
                                            SELECT Id
                                            FROM Lockers
                                            WHERE IsLockerInProcess = 1
                                            AND TIMESTAMPDIFF(SECOND, LockerInProcessTime, NOW()) > @TimeInterval;

                                            UPDATE Lockers
                                            SET IsLockerInProcess = 0, LockerInProcessTime = NULL
                                            WHERE Id IN(SELECT Id FROM LockerIDs);

                                            DELETE FROM TimeReservedLocks
                                            WHERE LockerId IN(SELECT Id FROM LockerIDs);";
                        cmd.Parameters.AddWithValue("@TimeInterval", _lockIdleInterval);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}
