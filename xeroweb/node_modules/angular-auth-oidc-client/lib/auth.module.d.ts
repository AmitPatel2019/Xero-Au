import { InjectionToken, ModuleWithProviders, Provider } from '@angular/core';
import { Observable } from 'rxjs';
import { OidcConfigService } from './config/config.service';
import { StsConfigLoader, StsConfigStaticLoader } from './config/loader/config-loader';
import { OpenIdConfiguration } from './config/openid-configuration';
import * as i0 from "@angular/core";
import * as i1 from "@angular/common";
import * as i2 from "@angular/common/http";
export interface PassedInitialConfig {
    config?: OpenIdConfiguration | OpenIdConfiguration[];
    loader?: Provider;
    storage?: any;
}
export declare function createStaticLoader(passedConfig: PassedInitialConfig): StsConfigStaticLoader;
export declare function configurationProviderFactory(oidcConfigService: OidcConfigService, loader: StsConfigLoader): () => Observable<OpenIdConfiguration[]>;
export declare const PASSED_CONFIG: InjectionToken<PassedInitialConfig>;
export declare class AuthModule {
    static forRoot(passedConfig: PassedInitialConfig): ModuleWithProviders<AuthModule>;
    static ɵfac: i0.ɵɵFactoryDeclaration<AuthModule, never>;
    static ɵmod: i0.ɵɵNgModuleDeclaration<AuthModule, never, [typeof i1.CommonModule, typeof i2.HttpClientModule], never>;
    static ɵinj: i0.ɵɵInjectorDeclaration<AuthModule>;
}
