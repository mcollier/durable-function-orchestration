$APPLICATION_NAME = "mcollier-durable-function-orchestration-github-oidc"
$GITHUB_REPO = "mcollier/durable-function-orchestration"

Write-Host "Please be sure you're logged into the Azure CLI and GitHub CLI!!"

# Check if the Azure CLI is installed
if (-not (Get-Command "az" -ErrorAction SilentlyContinue)) {
	Write-Host "Azure CLI is not installed. Please install it before running this script."
	exit
}
# Check if the GitHub CLI is installed
if (-not (Get-Command "gh" -ErrorAction SilentlyContinue)) {
	Write-Host "GitHub CLI is not installed. Please install it before running this script."
	exit
}

# Check if the user is logged into the Azure CLI
$azAccount = $(az account show --query id -o tsv)
if (-not $azAccount) {
	Write-Host "Please log into the Azure CLI before running this script."
	exit
}

# Check if the user is logged into the GitHub CLI
$ghAuthStatus = gh auth status 2>&1
if ($ghAuthStatus -match "not logged in") {
	Write-Host $ghAuthStatus
	Write-Host "Please log into the GitHub CLI before running this script."
	exit
}

$AZURE_SUBSCRIPTION_ID = $(az account show --query id -o tsv)
$AZURE_TENANT_ID = $(az account show --query tenantId -o tsv)

Write-Host "Using AZURE_SUBSCRIPTION_ID $AZURE_SUBSCRIPTION_ID"
Write-Host "Using AZURE_TENANT_ID $AZURE_TENANT_ID"

# # Create the EntraID application
Write-Host "Creating the EntraID application."
$APP_ID = $(az ad app create --display-name ${APPLICATION_NAME} --query appId -o tsv)

Start-Sleep -Seconds 5
Write-Host "Using APP_ID $APP_ID"

$APPLICATION_OBJECT_ID = $(az ad app show --id $APP_ID --query id -o tsv)
Write-Host "Using APPLICATION_OBJECT_ID $APPLICATION_OBJECT_ID"

# Create a service principal for the EntraID application
Write-Host "Creating a service principal for the EntraID application."
$SERVICE_PRINCIPAL_ID = $(az ad sp create --id $APP_ID --query id -o tsv)

Start-Sleep -Seconds 5
Write-Host "Using SERVICE_PRINCIPAL_ID $SERVICE_PRINCIPAL_ID"

Write-Host "Creating role assignment for the service principal."
az role assignment create `
	--role Contributor `
	--subscription $AZURE_SUBSCRIPTION_ID `
	--assignee-object-id $SERVICE_PRINCIPAL_ID `
	--assignee-principal-type ServicePrincipal `
	--scope /subscriptions/$AZURE_SUBSCRIPTION_ID

# # Create the federated OpenID Connect identity credential
Write-Host "Creating the federated OpenID Connect identity credential."
az rest `
	--method POST `
	--uri "https://graph.microsoft.com/beta/applications/${APPLICATION_OBJECT_ID}/federatedIdentityCredentials" `
	--body "{'name':'refpathfic', 'issuer':'https://token.actions.githubusercontent.com', 'subject':'repo:${GITHUB_REPO}:ref:refs/heads/main', 'description':'main', 'audiences':['api://AzureADTokenExchange']}"

az rest `
	--method POST `
	--uri "https://graph.microsoft.com/beta/applications/${APPLICATION_OBJECT_ID}/federatedIdentityCredentials" `
	--body "{'name':'prfic', 'issuer':'https://token.actions.githubusercontent.com', 'subject':'repo:${GITHUB_REPO}:pull_request', 'description':'pull request', 'audiences':['api://AzureADTokenExchange']}"


Write-Host "Creating GitHub repository secrets"
gh secret set AZURE_CLIENT_ID --body $APP_ID --app actions --repo $GITHUB_REPO
gh secret set AZURE_SUBSCRIPTION_ID --body $AZURE_SUBSCRIPTION_ID --app actions --repo $GITHUB_REPO
gh secret set AZURE_TENANT_ID --body $AZURE_TENANT_ID --app actions --repo $GITHUB_REPO

Write-Host "GitHub $GITHUB_REPO secrets"
gh secret list --repo $GITHUB_REPO

Write-Host "Done!"