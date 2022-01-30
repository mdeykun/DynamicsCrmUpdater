namespace CrmWebResourcesUpdater.DataModels
{
    //
    // Summary:
    //     Identifies the type of identity provider used for authentication.
    public enum AuthenticationProviderType
    {
        //
        // Summary:
        //     No identity provider. Value = 0.
        None = 0,
        //
        // Summary:
        //     An Active Directory identity provider. Value = 1.
        ActiveDirectory = 1,
        //
        // Summary:
        //     A federated claims identity provider. Value = 2.
        Federation = 2,
        //
        // Summary:
        //     A Microsoft account identity provider. Value = 3.
        LiveId = 3,
        //
        // Summary:
        //     An online (Office 365) federated identity provider. Value = 4.
        OnlineFederation = 4
    }
}
