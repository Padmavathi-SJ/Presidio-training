import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FieldLocationComponent } from './field-location.component';

describe('FieldLocationComponent', () => {
  let component: FieldLocationComponent;
  let fixture: ComponentFixture<FieldLocationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FieldLocationComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(FieldLocationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
