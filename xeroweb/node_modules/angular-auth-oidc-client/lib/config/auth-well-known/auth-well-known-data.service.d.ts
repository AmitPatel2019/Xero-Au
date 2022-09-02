import { Observable } from 'rxjs';
import { DataService } from '../../api/data.service';
import { AuthWellKnownEndpoints } from './auth-well-known-endpoints';
import * as i0 from "@angular/core";
export declare class AuthWellKnownDataService {
    private readonly http;
    constructor(http: DataService);
    getWellKnownEndPointsFromUrl(authWellknownEndpoint: string, configId: string): Observable<AuthWellKnownEndpoints>;
    private getWellKnownDocument;
    static ɵfac: i0.ɵɵFactoryDeclaration<AuthWellKnownDataService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<AuthWellKnownDataService>;
}
