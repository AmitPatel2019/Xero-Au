import { Observable } from 'rxjs';
import { AuthStateService } from '../auth-state/auth-state.service';
import { AuthWellKnownService } from '../config/auth-well-known/auth-well-known.service';
import { ConfigurationProvider } from '../config/provider/config.provider';
import { FlowsDataService } from '../flows/flows-data.service';
import { RefreshSessionIframeService } from '../iframe/refresh-session-iframe.service';
import { SilentRenewService } from '../iframe/silent-renew.service';
import { LoggerService } from '../logging/logger.service';
import { LoginResponse } from '../login/login-response';
import { StoragePersistenceService } from '../storage/storage-persistence.service';
import { UserService } from '../user-data/user.service';
import { FlowHelper } from '../utils/flowHelper/flow-helper.service';
import { RefreshSessionRefreshTokenService } from './refresh-session-refresh-token.service';
import * as i0 from "@angular/core";
export declare const MAX_RETRY_ATTEMPTS = 3;
export declare class RefreshSessionService {
    private flowHelper;
    private configurationProvider;
    private flowsDataService;
    private loggerService;
    private silentRenewService;
    private authStateService;
    private authWellKnownService;
    private refreshSessionIframeService;
    private storagePersistenceService;
    private refreshSessionRefreshTokenService;
    private userService;
    constructor(flowHelper: FlowHelper, configurationProvider: ConfigurationProvider, flowsDataService: FlowsDataService, loggerService: LoggerService, silentRenewService: SilentRenewService, authStateService: AuthStateService, authWellKnownService: AuthWellKnownService, refreshSessionIframeService: RefreshSessionIframeService, storagePersistenceService: StoragePersistenceService, refreshSessionRefreshTokenService: RefreshSessionRefreshTokenService, userService: UserService);
    userForceRefreshSession(configId: string, extraCustomParams?: {
        [key: string]: string | number | boolean;
    }): Observable<LoginResponse>;
    forceRefreshSession(configId: string, extraCustomParams?: {
        [key: string]: string | number | boolean;
    }): Observable<LoginResponse>;
    private persistCustomParams;
    private startRefreshSession;
    private timeoutRetryStrategy;
    static ɵfac: i0.ɵɵFactoryDeclaration<RefreshSessionService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<RefreshSessionService>;
}
