import { Observable } from 'rxjs';
import { CallbackContext } from '../flows/callback-context';
import { FlowsService } from '../flows/flows.service';
import { ResetAuthDataService } from '../flows/reset-auth-data.service';
import { LoggerService } from '../logging/logger.service';
import { IntervalService } from './interval.service';
import * as i0 from "@angular/core";
export declare class RefreshSessionRefreshTokenService {
    private loggerService;
    private resetAuthDataService;
    private flowsService;
    private intervalService;
    constructor(loggerService: LoggerService, resetAuthDataService: ResetAuthDataService, flowsService: FlowsService, intervalService: IntervalService);
    refreshSessionWithRefreshTokens(configId: string, customParamsRefresh?: {
        [key: string]: string | number | boolean;
    }): Observable<CallbackContext>;
    static ɵfac: i0.ɵɵFactoryDeclaration<RefreshSessionRefreshTokenService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<RefreshSessionRefreshTokenService>;
}
