import { ConfigurationProvider } from '../../config/provider/config.provider';
import * as i0 from "@angular/core";
export declare class FlowHelper {
    private configurationProvider;
    constructor(configurationProvider: ConfigurationProvider);
    isCurrentFlowCodeFlow(configId: string): boolean;
    isCurrentFlowAnyImplicitFlow(configId: string): boolean;
    isCurrentFlowCodeFlowWithRefreshTokens(configId: string): boolean;
    isCurrentFlowImplicitFlowWithAccessToken(configId: string): boolean;
    currentFlowIs(flowTypes: string[] | string, configId: string): boolean;
    private isCurrentFlowImplicitFlowWithoutAccessToken;
    static ɵfac: i0.ɵɵFactoryDeclaration<FlowHelper, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<FlowHelper>;
}
