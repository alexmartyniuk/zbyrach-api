import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MailingSettingsComponent } from './mailing-settings.component';

describe('MailingSettingsComponent', () => {
  let component: MailingSettingsComponent;
  let fixture: ComponentFixture<MailingSettingsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MailingSettingsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MailingSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
