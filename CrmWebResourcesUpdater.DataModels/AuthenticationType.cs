namespace CrmWebResourcesUpdater.DataModels
{
    //
    // Summary:
    //     Decision switch for the sort of Auth to login to CRM with
    public enum AuthenticationType
    {
        //
        // Summary:
        //     Invalid connection
        InvalidConnection = -1,
        //
        // Summary:
        //     Active Directory Auth
        AD = 0,
        //
        // Summary:
        //     Live Auth
        Live = 1,
        //
        // Summary:
        //     SPLA Auth
        IFD = 2,
        //
        // Summary:
        //     CLAIMS based Auth
        Claims = 3,
        //
        // Summary:
        //     Office365 base login process
        Office365 = 4,
        //
        // Summary:
        //     OAuth based Auth
        OAuth = 5,
        //
        // Summary:
        //     Certificate based Auth
        Certificate = 6,
        //
        // Summary:
        //     Client Id + Secret Auth type.
        ClientSecret = 7,
        //
        // Summary:
        //     Enabled Host to manage Auth token for CRM connections.
        ExternalTokenManagement = 99
    }
}
