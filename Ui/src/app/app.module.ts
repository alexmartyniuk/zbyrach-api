import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { TagInputModule } from 'ngx-chips';
import { TagsSelectorComponent } from './tags-selector/tags-selector.component';
import { TagComponent } from './tag/tag.component';


@NgModule({
  declarations: [
    AppComponent,
    TagsSelectorComponent,
    TagComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule, 
    FormsModule, 
    ReactiveFormsModule, 
    TagInputModule, 
    HttpClientModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
