import { Observable } from 'rxjs';
import { PublicEventsService } from '../../public-events/public-events.service';
import { StoragePersistenceService } from '../../storage/storage-persistence.service';
import { AuthWellKnownDataService } from './auth-well-known-data.service';
import { AuthWellKnownEndpoints } from './auth-well-known-endpoints';
import * as i0 from "@angular/core";
export declare class AuthWellKnownService {
    private publicEventsService;
    private dataService;
    private storagePersistenceService;
    constructor(publicEventsService: PublicEventsService, dataService: AuthWellKnownDataService, storagePersistenceService: StoragePersistenceService);
    getAuthWellKnownEndPoints(authWellknownEndpointUrl: string, configId: string): Observable<AuthWellKnownEndpoints>;
    storeWellKnownEndpoints(configId: string, mappedWellKnownEndpoints: AuthWellKnownEndpoints): void;
    private getWellKnownEndPointsFromUrl;
    static ɵfac: i0.ɵɵFactoryDeclaration<AuthWellKnownService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<AuthWellKnownService>;
}
