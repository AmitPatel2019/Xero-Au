import { Observable } from 'rxjs';
import { DataService } from '../../api/data.service';
import { LoggerService } from '../../logging/logger.service';
import { StoragePersistenceService } from '../../storage/storage-persistence.service';
import { UrlService } from '../../utils/url/url.service';
import { ParResponse } from './par-response';
import * as i0 from "@angular/core";
export declare class ParService {
    private loggerService;
    private urlService;
    private dataService;
    private storagePersistenceService;
    constructor(loggerService: LoggerService, urlService: UrlService, dataService: DataService, storagePersistenceService: StoragePersistenceService);
    postParRequest(configId: string, customParams?: {
        [key: string]: string | number | boolean;
    }): Observable<ParResponse>;
    static ɵfac: i0.ɵɵFactoryDeclaration<ParService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<ParService>;
}
