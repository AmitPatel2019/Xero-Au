import { OpenIdConfiguration } from '../openid-configuration';
import * as i0 from "@angular/core";
export declare class ConfigurationProvider {
    private configsInternal;
    hasAtLeastOneConfig(): boolean;
    hasManyConfigs(): boolean;
    setConfig(readyConfig: OpenIdConfiguration): void;
    getOpenIDConfiguration(configId?: string): OpenIdConfiguration;
    getAllConfigurations(): OpenIdConfiguration[];
    static ɵfac: i0.ɵɵFactoryDeclaration<ConfigurationProvider, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<ConfigurationProvider>;
}
