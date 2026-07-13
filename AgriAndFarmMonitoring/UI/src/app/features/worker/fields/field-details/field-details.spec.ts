import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FieldDetails } from './field-details';

describe('FieldDetails', () => {
  let component: FieldDetails;
  let fixture: ComponentFixture<FieldDetails>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FieldDetails],
    }).compileComponents();

    fixture = TestBed.createComponent(FieldDetails);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
