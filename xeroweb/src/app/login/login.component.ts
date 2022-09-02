import { Component, OnInit, Output, EventEmitter, ViewEncapsulation } from '@angular/core';
import { Validators, FormControl, FormGroup, FormBuilder } from '@angular/forms';
import { Message, SelectItem, MessagesModule } from 'primeng/primeng';
import { Router, ActivatedRoute, Params, Data } from '@angular/router';
import { NgxSpinnerService } from 'ngx-spinner';
import { ApiService } from '../api.service';
import { EncryptingService } from '../encrypting.service';
import { StoreService } from '../store.service';
import './login.component.css';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  //styleUrls: ['./login.component.css'],
  providers: [ApiService, EncryptingService],
  encapsulation: ViewEncapsulation.None
})
export class LoginComponent implements OnInit {

  public loginform: FormGroup;
  public loading: boolean;
  public msgs: Message[] = [];

  public responseDB: any;
  public resXero: any;

  @Output() talk: EventEmitter<string> = new EventEmitter<string>();

  constructor( private fb: FormBuilder, private router: Router, private spinner: NgxSpinnerService,
    private api: ApiService, private encrypt: EncryptingService, private ss: StoreService ) {
    this.loginform = this.fb.group({
      UserName: ["", Validators.required],
      Password: ["", Validators.required]
    });

  }

  ngOnInit() {
  
    this.ss.clearAll();
    this.talk.emit('out');
  }


  txtFocus() {
    this.msgs = [];
  }


  onSubmit(value: any) {
    this.msgs = [];
    if (this.loginform.valid) {
      event.preventDefault();
      this.loading = true;
      this.spinner.show();

var encryptPassword = this.encrypt.encrypt(this.loginform.value.Password);
      var loginData = { 'UserName': this.loginform.value.UserName, 'Password': encryptPassword }
      this.ss.storePassword( encryptPassword);
      console.log('>>>>>>>>>>login screen');
      this.api.postAsEndUser('login/DoLogin', loginData).subscribe(
        (res: {}) => this.validateSave(res),
        error => this.validateSave(<any>error));

    }
  }
  GetAccount(accountRes:any)
  {
    console.log('GetAccounts entered');
    this.api.get('Xero/GetByAccountID', '').subscribe(
      (res: {}) => this.SaveAccount(res,accountRes),
      error => this.failedGetAccount(<any>error));
  }
  SaveAccount(res: any,accountRes : any) {
    console.log("res ",JSON.stringify(res));
    if (res.Data != null) {

      if (res.Data.length > 0) {
        this.ss.storeXeroConnectID(res.Data[0].XeroID);
        this.ss.storeCompanyName(res.Data[0].CompanyName);
        this.router.navigate(['/initlogin/'+accountRes.Data.Token.toString()+'/0/login']);
      }
    }

  }

  failedGetAccount(re:any)
  {
console.log(JSON.stringify(re));
  }
  validateError(res: any) {
    this.loading = false;
    this.spinner.hide();
    this.msgs.push({ severity: 'error', summary: 'Oops..', detail: 'Invalid Credentials' });
  }

  validateSave(res: any) {
    this.spinner.hide();

    console.log(res);
    this.loading = false;
    if (res.StatusCode == 0) {
      this.ss.storeUserName(res.Data.UserName.toString());
      this.ss.storeToken(res.Data.Token.toString());
      this.ss.storeEmail(res.Data.EmailAddress.toString());
      this.resXero = res.Data;
      this.GetAccount(res);
      


    } else {
      this.msgs.push({ severity: 'error', summary: 'Oops..', detail: 'Incorrect credentials' });
    }
  }




}


