import { HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigurationProvider } from '../config/provider/config.provider';
import { HttpBaseService } from './http-base.service';
import * as i0 from "@angular/core";
export declare class DataService {
    private httpClient;
    private readonly configurationProvider;
    constructor(httpClient: HttpBaseService, configurationProvider: ConfigurationProvider);
    get<T>(url: string, configId: string, token?: string): Observable<T>;
    post<T>(url: string, body: any, configId: string, headersParams?: HttpHeaders): Observable<T>;
    private prepareHeaders;
    private prepareParams;
    static ɵfac: i0.ɵɵFactoryDeclaration<DataService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<DataService>;
}
