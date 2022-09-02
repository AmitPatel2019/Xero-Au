import { ConfigurationProvider } from '../config/provider/config.provider';
import * as i0 from "@angular/core";
export declare class ClosestMatchingRouteService {
    private configProvider;
    constructor(configProvider: ConfigurationProvider);
    getConfigIdForClosestMatchingRoute(route: string): ClosestMatchingRouteResult;
    private getAllConfiguredRoutes;
    static ɵfac: i0.ɵɵFactoryDeclaration<ClosestMatchingRouteService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<ClosestMatchingRouteService>;
}
export interface ConfiguredRoutesWithConfig {
    routes: string[];
    configId: string;
}
export interface ClosestMatchingRouteResult {
    matchingRoute: string;
    matchingConfigId: string;
}
