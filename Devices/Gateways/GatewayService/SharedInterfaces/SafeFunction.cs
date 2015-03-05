namespace Microsoft.ConnectTheDots.Common
{
    using System;

    //--//

    public class SafeFunc<TResult>
    {
        Func<TResult> _function;
        ILogger _logger;

        //--//

        public SafeFunc( Func<TResult> function, ILogger logger )
        {
            _function = function;
            _logger = SafeLogger.FromLogger( logger );
        }

        public TResult SafeInvoke( )
        {
            try
            {
                return _function( );
            }
            catch( Exception ex )
            {
                _logger.LogError( "Exception in task: " + ex.StackTrace );
            }

            return default( TResult );
        }
    }
}
