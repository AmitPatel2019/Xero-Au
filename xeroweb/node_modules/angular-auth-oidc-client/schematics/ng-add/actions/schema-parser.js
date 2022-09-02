"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.parseSchema = void 0;
const schema_1 = require("../schema");
const AUTH_CONFIG_MODULE = { moduleFileName: 'auth-config.module', moduleName: 'AuthConfigModule', moduleFolder: 'auth-config' };
const AUTH_HTTP_CONFIG_MODULE = {
    moduleFileName: 'auth-http-config.module',
    moduleName: 'AuthHttpConfigModule',
    moduleFolder: 'auth-http-config',
};
function needsHttp(flowType) {
    return flowType === schema_1.FlowType.OidcCodeFlowPkceUsingIframeSilentRenewGettingConfigFromHttp;
}
function needsSilentRenewHtml(flowType) {
    const optionsWithSilentRenewHtml = [
        schema_1.FlowType.OidcCodeFlowPkceUsingIframeSilentRenewGettingConfigFromHttp,
        schema_1.FlowType.OidcCodeFlowPkceAzureAdUsingIframeSilentRenew,
        schema_1.FlowType.OidcCodeFlowPkceUsingIframeSilentRenew,
    ];
    return optionsWithSilentRenewHtml.includes(flowType);
}
function getModuleInfo(flowType) {
    if (needsHttp(flowType)) {
        return AUTH_HTTP_CONFIG_MODULE;
    }
    return AUTH_CONFIG_MODULE;
}
function parseSchema(options) {
    const { flowType } = options;
    return Object.assign(Object.assign({}, options), { moduleInfo: getModuleInfo(flowType), isHttpOption: needsHttp(flowType), needsSilentRenewHtml: needsSilentRenewHtml(flowType) });
}
exports.parseSchema = parseSchema;
//# sourceMappingURL=schema-parser.js.map