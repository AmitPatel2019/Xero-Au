import { Observable } from 'rxjs';
import { DataService } from '../api/data.service';
import { AuthOptions } from '../auth-options';
import { ConfigurationProvider } from '../config/provider/config.provider';
import { ResetAuthDataService } from '../flows/reset-auth-data.service';
import { CheckSessionService } from '../iframe/check-session.service';
import { LoggerService } from '../logging/logger.service';
import { StoragePersistenceService } from '../storage/storage-persistence.service';
import { RedirectService } from '../utils/redirect/redirect.service';
import { UrlService } from '../utils/url/url.service';
import * as i0 from "@angular/core";
export declare class LogoffRevocationService {
    private dataService;
    private storagePersistenceService;
    private loggerService;
    private urlService;
    private checkSessionService;
    private resetAuthDataService;
    private redirectService;
    private configurationProvider;
    constructor(dataService: DataService, storagePersistenceService: StoragePersistenceService, loggerService: LoggerService, urlService: UrlService, checkSessionService: CheckSessionService, resetAuthDataService: ResetAuthDataService, redirectService: RedirectService, configurationProvider: ConfigurationProvider);
    logoff(configId: string, authOptions?: AuthOptions): void;
    logoffLocal(configId: string): void;
    logoffLocalMultiple(): void;
    logoffAndRevokeTokens(configId: string, authOptions?: AuthOptions): Observable<any>;
    revokeAccessToken(configId: string, accessToken?: any): Observable<any>;
    revokeRefreshToken(configId: string, refreshToken?: any): Observable<any>;
    getEndSessionUrl(configId: string, customParams?: {
        [p: string]: string | number | boolean;
    }): string | null;
    private sendRevokeRequest;
    static ɵfac: i0.ɵɵFactoryDeclaration<LogoffRevocationService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<LogoffRevocationService>;
}
