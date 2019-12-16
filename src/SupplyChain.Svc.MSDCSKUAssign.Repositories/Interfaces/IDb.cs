using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Repositories.Interfaces
{
    public interface IDb : IDisposable
    {
        /// <summary>
        /// <para>Gets the string used to open a database.</para>
        /// </summary>
        /// <value>
        /// <para>The string used to open a database.</para>
        /// </value>
        /// <seealso cref="DbConnection.ConnectionString"/>
        string ConnectionString { get; }

        /// <summary>
        /// <para>Creates a connection for this database.</para>
        /// </summary>
        /// <returns>
        /// <para>The <see cref="IDbConnection"/> for this database.</para>
        /// </returns>
        /// <seealso cref="IDbConnection"/>        
        IDbConnection DbConnection { get; }

        /// <summary>
        /// Gets a DbDataAdapter with Standard update behavior.
        /// </summary>
        /// <returns>A <see cref="IDbDataAdapter"/>.</returns>
        /// <seealso cref="IDbDataAdapter"/>
        /// <devdoc>
        /// Created this new, public method instead of modifying the protected, abstract one so that there will be no
        /// breaking changes for any currently derived Database class.
        /// </devdoc>
        IDbDataAdapter GetDataAdapter();

        /// <summary>
        /// <para>Creates a <see cref="IDbCommand"/> for a SQL query.</para>
        /// </summary>
        /// <param name="query"><para>The text of the query.</para></param>        
        /// <returns><para>The <see cref="IDbCommand"/> for the SQL query.</para></returns>        
        IDbCommand GetStringCommand(string query);

        /// <summary>
        /// <para>Creates a <see cref="IDbCommand"/> for a stored procedure.</para>
        /// </summary>
        /// <param name="storedProcedureName"><para>The name of the stored procedure.</para></param>
        /// <returns><para>The <see cref="IDbCommand"/> for the stored procedure.</para></returns>       
        IDbCommand GetStoredProcCommand(string storedProcedureName);


        /// <summary>
        /// Adds the structered parameter.
        /// </summary>
        /// <param name="dbCommand">The database command.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        void AddInStructuredParameter(IDbCommand dbCommand, string name, object value);

        /// <summary>
        /// Adds the in parameter.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="name">The name.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="value">The value.</param>
        void AddInParameter(IDbCommand command, string name, DbType dbType, object value);

        /// <summary>
        /// Adds the in parameter.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="name">The name.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="value">The value.</param>
        void AddInParameter(IDbCommand command, string name, DbType dbType, int size, object value);

        /// <summary>
        /// Adds the in parameter.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="name">The name.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="value">The value.</param>
        IDbDataParameter AddOutParameter(IDbCommand command, string name, DbType dbType);

        /// <summary>
        /// Adds the in parameter.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="name">The name.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="value">The value.</param>
        IDbDataParameter AddReturnValueParameter(IDbCommand command, string name, DbType dbType);

        /// <summary>
        /// Adds the in parameter.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="name">The name.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="value">The value.</param>
        IDbDataParameter AddInOutParameter(IDbCommand command, string name, DbType dbType, object value);

        /// <summary>
        /// Gets the data adapter.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        IDataAdapter GetDataAdapter(IDbCommand command);


        /// <summary>
        /// Closes this instance.
        /// </summary>
        void Close();
    }
}
