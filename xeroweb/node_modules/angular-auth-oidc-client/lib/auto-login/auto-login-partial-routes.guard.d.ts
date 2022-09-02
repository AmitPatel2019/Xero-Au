import { ActivatedRouteSnapshot, CanActivate, CanActivateChild, CanLoad, Route, RouterStateSnapshot, UrlSegment } from '@angular/router';
import { AuthStateService } from '../auth-state/auth-state.service';
import { ConfigurationProvider } from '../config/provider/config.provider';
import { LoginService } from '../login/login.service';
import { AutoLoginService } from './auto-login.service';
import * as i0 from "@angular/core";
export declare class AutoLoginPartialRoutesGuard implements CanActivate, CanActivateChild, CanLoad {
    private autoLoginService;
    private authStateService;
    private loginService;
    private configurationProvider;
    constructor(autoLoginService: AutoLoginService, authStateService: AuthStateService, loginService: LoginService, configurationProvider: ConfigurationProvider);
    canLoad(route: Route, segments: UrlSegment[]): boolean;
    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean;
    canActivateChild(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean;
    private checkAuth;
    private getId;
    static ɵfac: i0.ɵɵFactoryDeclaration<AutoLoginPartialRoutesGuard, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<AutoLoginPartialRoutesGuard>;
}
