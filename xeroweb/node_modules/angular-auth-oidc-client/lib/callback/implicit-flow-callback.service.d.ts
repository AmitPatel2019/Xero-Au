import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { ConfigurationProvider } from '../config/provider/config.provider';
import { CallbackContext } from '../flows/callback-context';
import { FlowsDataService } from '../flows/flows-data.service';
import { FlowsService } from '../flows/flows.service';
import { IntervalService } from './interval.service';
import * as i0 from "@angular/core";
export declare class ImplicitFlowCallbackService {
    private flowsService;
    private configurationProvider;
    private router;
    private flowsDataService;
    private intervalService;
    constructor(flowsService: FlowsService, configurationProvider: ConfigurationProvider, router: Router, flowsDataService: FlowsDataService, intervalService: IntervalService);
    authenticatedImplicitFlowCallback(configId: string, hash?: string): Observable<CallbackContext>;
    static ɵfac: i0.ɵɵFactoryDeclaration<ImplicitFlowCallbackService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<ImplicitFlowCallbackService>;
}
