const proxyConfig = require('../proxy.config.js')

const spaUrl = existedOrDefault(proxyConfig.spaUrl, 'http://localhost:8084/')

export const oidcSettings = {
  authority: '/config',
  client_id: 'spa',
  redirect_uri: `${spaUrl.target}callback`,
  response_type: 'token id_token',
  automaticSilentRenew: true,
  post_logout_redirect_uri: spaUrl.target,
  silent_redirect_uri: `${spaUrl.target}callback`,
  scope: 'inventory_api_scope cart_api_scope catalog_api_scope rating_api_scope openid profile'
}

function existedOrDefault(variable, defaultOne) {
  if (variable == null || variable == 'undefined') {
    return defaultOne
  } else {
    return variable
  }
}
