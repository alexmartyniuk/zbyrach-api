import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MyTagComponent } from './my-tag.component';

describe('MyTagComponent', () => {
  let component: MyTagComponent;
  let fixture: ComponentFixture<MyTagComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MyTagComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MyTagComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
