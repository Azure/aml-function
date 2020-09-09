[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure%2Faml-function%2Fmaster%2F.cloud%2F.azure%2Fazuredeploy.json)

# aml_function 
This repository contains an Azure Function app which contains an Http Trigger function. The function can send github repository dispatch event when triggered. The function name is generic_trigger function which can be invoked by sending a POST request. The function has a specified set of Azure event sources, if the request body contains any of these event sources then it converts the event_type to an appropriate format understandable by Github workflows and sends the repository dispatch event to the Github repository specified in the request parameter. 
This function-app once deployed, can be registered to event grid either manually or with ARM template by specifying the function endpoint as webhook url of event subscription using GitHub workflow.

### 1. Prerequisites

 The following prerequisites are required to make this repository work:
- Azure subscription
- Contributor access to the Azure subscription
- Access to [GitHub Actions](https://github.com/features/actions)

If you don’t have an Azure subscription, create a free account before you begin. Get started today with a [free Azure account](https://azure.com/free/open-source)!

### 2. Setting up the required secrets

#### To allow GitHub Actions to access Azure
An [Azure service principal](https://docs.microsoft.com/en-us/azure/active-directory/develop/app-objects-and-service-principals) needs to be generated. Just go to the Azure Portal to find the details of your resource group. Then start the Cloud CLI or install the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest) on your computer and execute the following command to generate the required credentials:

```sh
# Replace {service-principal-name}, {subscription-id} and {resource-group} with your 
# Azure subscription id and resource group name and any name for your service principle
az ad sp create-for-rbac --name {service-principal-name} \
                         --role contributor \
                         --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group} \
                         --sdk-auth
```

This will generate the following JSON output:

```sh
{
  "clientId": "<GUID>",
  "clientSecret": "<GUID>",
  "subscriptionId": "<GUID>",
  "tenantId": "<GUID>",
  (...)
}
```

Add this JSON output as [a secret](https://help.github.com/en/actions/configuring-and-managing-workflows/creating-and-storing-encrypted-secrets#creating-encrypted-secrets) with the name `AZURE_CREDENTIALS` in your GitHub repository:

To do so, click on the Settings tab in your repository, then click on Secrets and finally add the new secret with the name `AZURE_CREDENTIALS` to your repository.

Please follow [this link](https://help.github.com/en/actions/configuring-and-managing-workflows/creating-and-storing-encrypted-secrets#creating-encrypted-secrets) for more details. 

#### To Allow Azure to trigger a GitHub Workflow
 We also need GH PAT token with `repo` access so that function can trigger the appropriate workflow by sending the repository dispatch event to Github Repository with this PAT token. 
 
 Add the PAT token with as [a secret](https://help.github.com/en/actions/configuring-and-managing-workflows/creating-and-storing-encrypted-secrets#creating-encrypted-secrets) with the name `PAT_TOKEN` in your GitHub repository:


#### parameters_file (Parameters File)

A sample file can be found in this repository in the folder `.cloud/.azure`. The JSON file can include the following parameters:

| Parameter             | Required|  Default | Description |
| -------------------   | --------|  ----------| ----------- |
| functionAppName       |  x      |  fappDeploy | Function app name |
| functionGitHubURL     |  x      |  https://github.com/mlopstemplates/function_app.git | Github URL of the function app repository. |
| functionGitHubBranch  |  x      |  master     | Github branch of the function app repository. |
| functionFolder        |  x      |  fappdeploy | Folder name of the Github repository which contains the function app. |
| ownerName             |  x      |  amlfunction| The owner name of the function app. |
| patToken              |  x      |  PAT_TOKEN  | This is a Github Personal Access Token with `repo` access, stored as a secret in the Github Repository where this template is being used.. |


### Basic Requirements to use the Deploy to Azure button (Mandatory):
  1. **Subscription**: You need to select a subscription plan in order to deploy to Azure. Get started today with a [free Azure account](https://azure.com/free/open-source)!
  2. **Resource Group**: You can either select an existing resource group or create a new one.
  3. **Pat Token**: You need to provide the personal access token.
  
  ##### Other optional configurable parameters to use with Deploy to Azure button
   1. **Function App Name**: Name use as base-template to name the resources to be deployed in Azure (Default = fappDeploy).
   2. **Function Git Hub URL**: The URL of GitHub (ending by .git)
   3. **Function Git Hub Branch**: Name of the branch to use when deploying (Default = master).
   4. **Function Folder**: The name of folder containing the function code (Default = fappDeploy).
   5. **Owner Name**: Owner of this deployment, person to contact for question.    
   6. **Expire On**: Just a text value (format: yyyy-MM-dd) that express when it is safe to delete these resources.

### Events and its corresponding event types sent by the event-grid: [Link](https://docs.microsoft.com/en-us/azure/event-grid/overview#event-sources)
```sh
Azure App Configuration Events
  1.Microsoft.AppConfiguration.KeyValueModified: appconfiguration-keyvaluemodified
  2.Microsoft.AppConfiguration.KeyValueDeleted: appconfiguration-keyvaluedeleted
  
Azure App Service Events
  1.Microsoft.Web/sites.BackupOperationStarted: web/sites-backupoperationstarted
  2.Microsoft.Web/sites.BackupOperationCompleted: web/sites-backupoperationcompleted	
  3.Microsoft.Web/sites.BackupOperationFailed: web/sites-backupoperationfailed	
  4.Microsoft.Web/sites.RestoreOperationStarted: web/sites-restoreoperationstarted
  5.Microsoft.Web/sites.RestoreOperationCompleted: web/sites-restoreoperationcompleted
  6.Microsoft.Web/sites.RestoreOperationFailed: web/sites-restoreoperationfailed
  7.Microsoft.Web/sites.SlotSwapStarted: web/sites-slowswapstarted
  8.Microsoft.Web/sites.SlotSwapCompleted: web/sites-slowswapcompleted
  10.Microsoft.Web/sites.SlotSwapFailed: web/sites-slowswapfailed
  11.Microsoft.Web/sites.SlotSwapWithPreviewStarted: web/sites-slowswapwithpreviewstarted
  12.Microsoft.Web/sites.SlotSwapWithPreviewCancelled: web/sites-slowswapwithpreviewcancelled
  13.Microsoft.Web/sites.AppUpdated.Restarted: web/sites-appupdated-restarted
  14.Microsoft.Web/sites.AppUpdated.Stopped:  web/sites-appupdated-stopped
  15.Microsoft.Web/sites.AppUpdated.ChangedAppSettings:  web/sites-appupdated-changeappsettings
  16.Microsoft.Web/serverfarms.AppServicePlanUpdated: web/serverfarms-appserviceplanupdated
  
Azure Blob Storage Events
  1.Microsoft.Storage.BlobCreated: storage-blobcreated
  2.Microsoft.Storage.BlobDeleted: storage-blobdeleted
  
Azure Container Registry Events
  1.Microsoft.ContainerRegistry.ImagePushed: containerregistery-imagepushed
  2.Microsoft.ContainerRegistry.ImageDeleted: containerregistery-imagedeleted
  3.Microsoft.ContainerRegistry.ChartPushed: containerregistery-chartpushed
  4.Microsoft.ContainerRegistry.ChartDeleted: containerregistery-chartdeleted

Azure Event Hubs
  1.Microsoft.EventHub.CaptureFileCreated: eventhub-capturefilecreated

Azure IoT Hub
  1.Microsoft.Devices.DeviceCreated: devices-devicecreated
  2.Microsoft.Devices.DeviceDeleted: devices-devicedeleted
  3.Microsoft.Devices.DeviceConnected: devices-deviceconnected
  4.Microsoft.Devices.DeviceDisconnected: devices-devicedisconnected
  5.Microsoft.Devices.DeviceTelemetry: devices-devicetelementry

Azure Key Vault
  1.Microsoft.KeyVault.CertificateNewVersionCreated:  keyvault-certificatenewversioncreated, keyvault-certificatenearexpiry, 
  2.Microsoft.KeyVault.CertificateNearExpiry: keyvault-certificatenearexpiry
  3.Microsoft.KeyVault.CertificateExpired: keyvault-certificateexpired
  4.Microsoft.KeyVault.KeyNewVersionCreated: keyvault-keynewversioncreated
  5.Microsoft.KeyVault.KeyNearExpiry: keyvault-keynearexpiry
  6.Microsoft.KeyVault.KeyExpired: keyvault-keyexpired
  7.Microsoft.KeyVault.SecretNewVersionCreated: keyvault-secretnewversioncreated
  8.Microsoft.KeyVault.SecretNearExpiry: keyvault-secretnearexpiry
  9.Microsoft.KeyVault.SecretExpired: keyvault-secretexpired
  
Azure Machine Learning Events
  1.Microsoft.MachineLearningServices.ModelRegistered: machinelearningservices-modelregistered
  2.Microsoft.MachineLearningServices.ModelDeployed: machinelearningservices-modeldeployed
  3.Microsoft.MachineLearningServices.RunCompleted: machinelearningservices-runcompleted
  4.Microsoft.MachineLearningServices.DatasetDriftDetected: machinelearningservices-datadriftdetected
  5.Microsoft.MachineLearningServices.RunStatusChanged: machinelearningservices-runstatuschanged

Azure Maps Events
  1.Microsoft.Maps.GeofenceEntere: maps-geofenceentered 
  2.Microsoft.Maps.GeofenceExited: maps-geofenceexited
  3.Microsoft.Maps.GeofenceResult: maps-geofenceresult
  
Azure Media Services Events
  1.Microsoft.Media.JobStateChange: media-jobstatechange
  2.Microsoft.Media.JobScheduled: media-jobscheduled
  3.Microsoft.Media.JobProcessing: media-jobprocessing
  4.Microsoft.Media.JobCanceling: media-jobcanceling
  5.Microsoft.Media.JobFinished: media-jobfinished
  6.Microsoft.Media.JobCanceled: media-jobcanceled
  7.Microsoft.Media.JobErrore: media-joberrored
  8.Microsoft.Media.JobStateChange: media-joboutputstatechange
  9.Microsoft.Media.JobOutputScheduled: media-joboutputscheduled
  10.Microsoft.Media.JobOutputProcessing: media-joboutputprocessing
  11.Microsoft.Media.JobOutputCanceling: media-joboutputcanceling
  12.Microsoft.Media.JobOutputFinished: media-joboutputfinished
  13.Microsoft.Media.JobOutputCanceled: media-joboutputcanceled
  14.Microsoft.Media.JobOutputErrore: media-joboutputerrored

Azure Service Bus Events
  1.Microsoft.ServiceBus.ActiveMessagesAvailableWithNoListeners: servicebus-activemessageavailablewithnolisteners
  2.Microsoft.ServiceBus.DeadletterMessagesAvailableWithNoListener:  servicebus-deadlettermessagesavailablewithnolisteners
  
Azure SignalR Service Events
  1.Microsoft.SignalRService.ClientConnectionConnected: signalrservice-clientconnectionconnected
  2.Microsoft.SignalRService.ClientConnectionDisconnected: signalrservice-clientconnectiondisconnected

```

### Example:
#### To send any event from event-grid to the function app:
You need to specify the repository name in the function URL, as request parameter **repoName**
```
Request Method: POST
Request URL: http://host/api/generic_triggers?repoName=TestGenericRepositoryDispatch

```


### Example:
#### To trigger the workflow of the repository provided in function app URL(repoName), when the Machine Learning model is registered:
```sh

  On:
    repository_dispatch:
        types: [machinelearning-modelregistered]
  (...)

```

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
