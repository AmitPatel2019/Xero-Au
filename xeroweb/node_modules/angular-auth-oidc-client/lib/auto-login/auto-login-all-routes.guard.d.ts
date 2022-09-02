import { ActivatedRouteSnapshot, CanActivate, CanActivateChild, CanLoad, Route, RouterStateSnapshot, UrlSegment, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { CheckAuthService } from '../check-auth.service';
import { ConfigurationProvider } from '../config/provider/config.provider';
import { LoginService } from '../login/login.service';
import { AutoLoginService } from './auto-login.service';
import * as i0 from "@angular/core";
export declare class AutoLoginAllRoutesGuard implements CanActivate, CanActivateChild, CanLoad {
    private autoLoginService;
    private checkAuthService;
    private loginService;
    private configurationProvider;
    constructor(autoLoginService: AutoLoginService, checkAuthService: CheckAuthService, loginService: LoginService, configurationProvider: ConfigurationProvider);
    canLoad(route: Route, segments: UrlSegment[]): Observable<boolean | UrlTree>;
    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean | UrlTree>;
    canActivateChild(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean | UrlTree>;
    private checkAuth;
    private getId;
    static ɵfac: i0.ɵɵFactoryDeclaration<AutoLoginAllRoutesGuard, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<AutoLoginAllRoutesGuard>;
}
