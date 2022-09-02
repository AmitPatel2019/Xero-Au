import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthStateService } from '../auth-state/auth-state.service';
import { ConfigurationProvider } from '../config/provider/config.provider';
import { LoggerService } from '../logging/logger.service';
import { ClosestMatchingRouteService } from './closest-matching-route.service';
import * as i0 from "@angular/core";
export declare class AuthInterceptor implements HttpInterceptor {
    private authStateService;
    private configurationProvider;
    private loggerService;
    private closestMatchingRouteService;
    constructor(authStateService: AuthStateService, configurationProvider: ConfigurationProvider, loggerService: LoggerService, closestMatchingRouteService: ClosestMatchingRouteService);
    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>>;
    static ɵfac: i0.ɵɵFactoryDeclaration<AuthInterceptor, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<AuthInterceptor>;
}
