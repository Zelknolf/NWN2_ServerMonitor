﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.5466
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NWN
{
    using System.Runtime.Serialization;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="NWGameServer", Namespace="http://schemas.datacontract.org/2004/07/NWN")]
    public partial class NWGameServer : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private uint ActivePlayerCountField;
        
        private uint BuildNumberField;
        
        private uint ExpansionsMaskField;
        
        private System.DateTime LastHeartbeatField;
        
        private bool LocalVaultField;
        
        private uint MaximumPlayerCountField;
        
        private string ModuleNameField;
        
        private bool OnlineField;
        
        private bool PrivateServerField;
        
        private string ProductField;
        
        private string ServerAddressField;
        
        private string ServerNameField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public uint ActivePlayerCount
        {
            get
            {
                return this.ActivePlayerCountField;
            }
            set
            {
                this.ActivePlayerCountField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public uint BuildNumber
        {
            get
            {
                return this.BuildNumberField;
            }
            set
            {
                this.BuildNumberField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public uint ExpansionsMask
        {
            get
            {
                return this.ExpansionsMaskField;
            }
            set
            {
                this.ExpansionsMaskField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.DateTime LastHeartbeat
        {
            get
            {
                return this.LastHeartbeatField;
            }
            set
            {
                this.LastHeartbeatField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool LocalVault
        {
            get
            {
                return this.LocalVaultField;
            }
            set
            {
                this.LocalVaultField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public uint MaximumPlayerCount
        {
            get
            {
                return this.MaximumPlayerCountField;
            }
            set
            {
                this.MaximumPlayerCountField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ModuleName
        {
            get
            {
                return this.ModuleNameField;
            }
            set
            {
                this.ModuleNameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool Online
        {
            get
            {
                return this.OnlineField;
            }
            set
            {
                this.OnlineField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool PrivateServer
        {
            get
            {
                return this.PrivateServerField;
            }
            set
            {
                this.PrivateServerField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Product
        {
            get
            {
                return this.ProductField;
            }
            set
            {
                this.ProductField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ServerAddress
        {
            get
            {
                return this.ServerAddressField;
            }
            set
            {
                this.ServerAddressField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ServerName
        {
            get
            {
                return this.ServerNameField;
            }
            set
            {
                this.ServerNameField = value;
            }
        }
    }
}


[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
[System.ServiceModel.ServiceContractAttribute(Namespace="http://api.mst.valhallalegends.com/NWNMasterServerAPI", ConfigurationName="INWNMasterServerAPI")]
public interface INWNMasterServerAPI
{
    
    [System.ServiceModel.OperationContractAttribute(Action="http://api.mst.valhallalegends.com/NWNMasterServerAPI/INWNMasterServerAPI/LookupS" +
        "erverByName", ReplyAction="http://api.mst.valhallalegends.com/NWNMasterServerAPI/INWNMasterServerAPI/LookupS" +
        "erverByNameResponse")]
    NWN.NWGameServer[] LookupServerByName(string Product, string ServerName);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://api.mst.valhallalegends.com/NWNMasterServerAPI/INWNMasterServerAPI/LookupS" +
        "erverByAddress", ReplyAction="http://api.mst.valhallalegends.com/NWNMasterServerAPI/INWNMasterServerAPI/LookupS" +
        "erverByAddressResponse")]
    NWN.NWGameServer LookupServerByAddress(string Product, string ServerAddress);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://api.mst.valhallalegends.com/NWNMasterServerAPI/INWNMasterServerAPI/GetOnli" +
        "neServerList", ReplyAction="http://api.mst.valhallalegends.com/NWNMasterServerAPI/INWNMasterServerAPI/GetOnli" +
        "neServerListResponse")]
    NWN.NWGameServer[] GetOnlineServerList(string Product);
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
public interface INWNMasterServerAPIChannel : INWNMasterServerAPI, System.ServiceModel.IClientChannel
{
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
public partial class NWNMasterServerAPIClient : System.ServiceModel.ClientBase<INWNMasterServerAPI>, INWNMasterServerAPI
{
    
    public NWNMasterServerAPIClient()
    {
    }
    
    public NWNMasterServerAPIClient(string endpointConfigurationName) : 
            base(endpointConfigurationName)
    {
    }
    
    public NWNMasterServerAPIClient(string endpointConfigurationName, string remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
    {
    }
    
    public NWNMasterServerAPIClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
    {
    }
    
    public NWNMasterServerAPIClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(binding, remoteAddress)
    {
    }
    
    public NWN.NWGameServer[] LookupServerByName(string Product, string ServerName)
    {
        return base.Channel.LookupServerByName(Product, ServerName);
    }
    
    public NWN.NWGameServer LookupServerByAddress(string Product, string ServerAddress)
    {
        return base.Channel.LookupServerByAddress(Product, ServerAddress);
    }
    
    public NWN.NWGameServer[] GetOnlineServerList(string Product)
    {
        return base.Channel.GetOnlineServerList(Product);
    }
}