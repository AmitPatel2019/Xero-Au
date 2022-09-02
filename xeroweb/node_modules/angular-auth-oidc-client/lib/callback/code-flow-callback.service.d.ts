import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { ConfigurationProvider } from '../config/provider/config.provider';
import { CallbackContext } from '../flows/callback-context';
import { FlowsDataService } from '../flows/flows-data.service';
import { FlowsService } from '../flows/flows.service';
import { IntervalService } from './interval.service';
import * as i0 from "@angular/core";
export declare class CodeFlowCallbackService {
    private flowsService;
    private flowsDataService;
    private intervalService;
    private configurationProvider;
    private router;
    constructor(flowsService: FlowsService, flowsDataService: FlowsDataService, intervalService: IntervalService, configurationProvider: ConfigurationProvider, router: Router);
    authenticatedCallbackWithCode(urlToCheck: string, configId: string): Observable<CallbackContext>;
    static ɵfac: i0.ɵɵFactoryDeclaration<CodeFlowCallbackService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<CodeFlowCallbackService>;
}
