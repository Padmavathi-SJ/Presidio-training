import { ComponentFixture, TestBed } from '@angular/core/testing';

import { QualityChecks } from './quality-checks';

describe('QualityChecks', () => {
  let component: QualityChecks;
  let fixture: ComponentFixture<QualityChecks>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [QualityChecks],
    }).compileComponents();

    fixture = TestBed.createComponent(QualityChecks);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
