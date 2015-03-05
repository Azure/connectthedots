namespace Microsoft.ConnectTheDots.Common
{
    using System;

    //--//

    public class SafeAction<TParam>
    {
        Action<TParam> _action;
        ILogger _logger;

        //--//

        public SafeAction( Action<TParam> action, ILogger logger )
        {
            _action = action;
            _logger = SafeLogger.FromLogger( logger );
        }

        public void SafeInvoke( TParam obj )
        {
            try
            {
                _action( obj );
            }
            catch(Exception ex)
            {
                _logger.LogError( "Exception in task: " + ex.StackTrace ); 
            }
        }
    }
}
